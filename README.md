## 运行环境说明

当前目录为linux桌面版程序代码，基于Qt6框架开发

目前只在Ubuntu 24.04 LTS x86_64上测试通过，其他平台未测试，不保证能正常运行。

## 关于特殊库的依赖

Qt、Boost、crypto++库是通过系统包管理工具安装，其它通过vcpkg管理。参考vcpkg.json。

## 生成native库

```bash
# 配置cmake
cmake --preset macos
# 生成native库
cmake --build --preset macos #--config Release
# 安装native库到指定目录
cmake --install build/macos --config Debug
```

### 创建xcframework

```bash
xcodebuild -create-xcframework -framework quantum/Frameworks/MTQuantum.framework -output quantum/Frameworks/MTQuantum.xcframework
```