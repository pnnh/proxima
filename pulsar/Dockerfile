FROM ubuntu:23.10

RUN apt-get update \
		&& apt-get install -y ca-certificates \
		&& apt-get install -y libc6 \
		&& rm -rf /var/lib/apt/lists/*

# 指定RUN工作目录
WORKDIR /home
# 指定运行时库加载目录
ENV LD_LIBRARY_PATH /opt/lib

# 拷贝程序
COPY pulsar /opt
# 拷贝资源文件
COPY assets /home/assets

# 启动程序
ENTRYPOINT ["/opt/bin/pulsar"]
