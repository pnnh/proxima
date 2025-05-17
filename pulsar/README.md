一个尝试用C++实现的服务接口程序

## 依赖

Ubuntu 23.04环境，需预先安装并配置vcpkg

## 构建

### Debug构建

```shell
# 执行配置
cmake --preset Debug
# 执行构建
cmake --build --preset Debug
```

### Release构建

```shell
# 执行配置
cmake --preset Release
# 仅构建
cmake --build --preset Release
# 构建并安装
cmake --build --preset Release --target install
```

## 生成docker镜像

先构建完成并安装，将会生成docker/release目录，里面包含运行时文件

```shell
# 复制资产到docker目录
cp -r assets docker
cd docker
sudo docker build -f Dockerfile -t dream-pulsar:latest .
# 测试执行构建的镜像
sudo docker run -p 8090:8080 dream-pulsar
# 仅在本地测试时使用
sudo docker run -p 8090:8080 dream-pulsar
```

## 关于folly库引用

目前folly库如果通过vcpkg引用，则会编译报错
通过手动在Ubuntu 23.10下编译并安装，则其会依赖fmt v9版本和spdlog v1.10版本
所以在vcpkg.json中引用spdlog时需要指定低版本，否则会因为版本过高，进而导致folly链接失败
