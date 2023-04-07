
# 大批量模型渲染

![GitHub](https://github.com/xieliujian/UnityDemo_GpuSkin/blob/main/Video/1.png?raw=true)

## 动画烘培到贴图

### 烘培骨骼数据到贴图

通过Animator的StartRecording和StopRecording函数，可以将一个动画数据一帧帧记录下来, 默认一秒是30帧，假如是一秒的动画，就是有30帧数据
 
获取模型每一帧每根骨骼的矩阵，然后按照顺序存储到贴图上，一个像素有RGBA四个通道，可以存储矩阵的一行数据，所以一根骨骼矩阵数据需要4个像素存储。存储贴图信息如下所示
 
frame1

bone1
 
(m00, m01, m02, m03) (m10, m11, m12, m13)(m20, m21,m22, m23)(m30, m31,m32,m33)
 
bone2 ....
 
frame2 ....

一个模型有多个动画的时候，需要记录一个配置信息，第一个动画的数据存到第几个像素，下个动画从这个像素开始继续存储数据就可以了。

这个就可以把一个动画的数据存储到贴图上

最终存储的骨骼数据贴图如下图所示

![GitHub](https://github.com/xieliujian/UnityDemo_GpuSkin/blob/main/Video/2.png?raw=true)

配置数据如下所示

![GitHub](https://github.com/xieliujian/UnityDemo_GpuSkin/blob/main/Video/3.png?raw=true)

那么存储到贴图的骨骼数据我们需要怎么读取呢

1. Mesh处理，mesh的uv2 存储骨骼的索引信息，uv3 存储 骨骼的权重信息

2. Shader读取

Shader中通过 当前动画的像素偏移，当前动画的帧索引，当前的骨骼索引，就可以从贴图中获取到当前帧的骨骼矩阵数据

假如一个顶点受4根骨骼影响，根据动画算法

matrix1 * weight1 + matrix2 * weight2 + matrix3 * weight3 + matrix4 * weight4

这样就能计算出最终的本地空间的顶点位置

### 动画信息烘培到贴图

通过Animator的StartRecording和StopRecording函数，可以将一个动画数据一帧帧记录下来, 默认一秒是30帧，假如是一秒的动画，就是有30帧数据

获取模型每一帧的模型顶点数据，然后按照顺序存储到贴图上，一个像素有RGBA四个通道，只需要
RGB通道就可以存储一个顶点信息

存储贴图信息如下所示

frame1

vertex1 vertex2 ...

frame2 

...

一个模型有多个动画的时候，需要记录一个配置信息，第一个动画的数据存到第几个像素，下个动画从这个像素开始继续存储数据就可以了。

这个就可以把一个动画的数据存储到贴图上

最终存储的顶点数据贴图如下图所示

![GitHub](https://github.com/xieliujian/UnityDemo_GpuSkin/blob/main/Video/4.png?raw=true)

配置数据如下所示

![GitHub](https://github.com/xieliujian/UnityDemo_GpuSkin/blob/main/Video/5.png?raw=true)

存储到贴图的顶点数据读取比较简单

Shader中通过 当前动画的像素偏移，当前动画的帧索引，当前的顶点索引，就可以从贴图中获取到当前帧当前顶点的顶点位置

### 使用骨骼烘培还是顶点烘培

标题  |   骨骼烘培 | 顶点烘培
----  |  ----   |  ---
存储贴图大小 | 骨骼烘培贴图小 | 顶点贴图大
Shader计算复杂度 | Shader需要采样骨骼信息计算顶点数据，Shader复杂度大 |  Shader只需要采样顶点数据，没有额外的计算
Mesh修改 | 需要修改原始Mesh，顶点上记录骨骼索引和骨骼权重 | 原始模型

如果项目需求是限制顶点数目，动画总时长少的情况下，比如1500个顶点，通过顶点烘培的贴图内存不大，使用顶点烘培有优势

### 烘培贴图的性能优势

因为把动画的最终数据烘培到贴图上，只需要在Shader中查找最终数据，不再需要Animator.Update实时计算骨骼信息，在有大批量动画的情况下，可以减少大量CPU动画计算耗时

## 材质实例化

unity 支持GPU instance功能，可以大量减少Draw Call

1. 材质支持

通过对材质勾选Enable GPU Instancing，就开启了材质Instance

2.  Shader支持

对于Shader中的实例化参数，需要用UNITY_INSTANCING_BUFFER_START(Props)和UNITY_INSTANCING_BUFFER_END(Props)标记，Shader中获取的时候，通过UNITY_ACCESS_INSTANCED_PROP接口去获取，如下图所示

![GitHub](https://github.com/xieliujian/UnityDemo_GpuSkin/blob/main/Video/6.png?raw=true)

![GitHub](https://github.com/xieliujian/UnityDemo_GpuSkin/blob/main/Video/7.png?raw=true)

![GitHub](https://github.com/xieliujian/UnityDemo_GpuSkin/blob/main/Video/8.png?raw=true)

3. 代码设置

unity提供了MaterialPropertyBlock，可以对同一个材质设置不同的属性值，如下图所示，Shader中传入每个实例的自己的参数值

![GitHub](https://github.com/xieliujian/UnityDemo_GpuSkin/blob/main/Video/9.png?raw=true)

如下图所示，创建200个测试Npc, Draw Call 数目只有3个，通过GPU instance, 可以减少大批量的Draw Call数，提高GPU的性能

![GitHub](https://github.com/xieliujian/UnityDemo_GpuSkin/blob/main/Video/10.png?raw=true)

## 游戏内脚本刷新耗时的优化方案

在手机上测试性能，以 Galaxy S8 这部手机为例，发现400个npc, Update耗时会占用 6 ~ 8ms, 
查看Update函数，就是每帧计算动画的帧信息，中低配手机，Update会占用大量的CPU时间

![GitHub](https://github.com/xieliujian/UnityDemo_GpuSkin/blob/main/Video/11.png?raw=true)

![GitHub](https://github.com/xieliujian/UnityDemo_GpuSkin/blob/main/Video/12.png?raw=true)

为了解决这个问题，只在每个脚本设置动画的时候设置一下参数值，去掉Update函数

![GitHub](https://github.com/xieliujian/UnityDemo_GpuSkin/blob/main/Video/13.png?raw=true)

然后在Shader中有一个全局帧，每隔一帧加一，达到变化动画的目的，这样就解决了脚本耗时的问题

![GitHub](https://github.com/xieliujian/UnityDemo_GpuSkin/blob/main/Video/14.png?raw=true)

![GitHub](https://github.com/xieliujian/UnityDemo_GpuSkin/blob/main/Video/15.png?raw=true)

## 总结

使用动画烘培 + GPU Instance方案，可以在游戏里面同屏展示多个动画实例

相比于Animator动画显示多个角色，有以下几方面优点

1. 没有动画刷新耗时，通过把动画烘培到贴图上，只增加了一些贴图内存

2. 启用GPU Instance方案，可以几个Draw Call 就渲染完多个角色，提升了GPU的性能

