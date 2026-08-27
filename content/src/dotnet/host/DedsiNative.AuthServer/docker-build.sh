#!/bin/bash

set -e

IMAGE_NAME="dedsinative-authserver"
CONTAINER_NAME="dedsinative-authserver-001"
HOST_PORT=23841
CONTAINER_PORT=8080

# Step 2: 检查并清理旧容器和镜像
if docker image inspect "${IMAGE_NAME}:latest" > /dev/null 2>&1; then
    echo ">>> 镜像 ${IMAGE_NAME}:latest 已存在，开始清理..."
    # 停止并删除关联的容器
    CONTAINERS=$(docker ps -a --filter "ancestor=${IMAGE_NAME}:latest" -q)
    if [ -n "$CONTAINERS" ]; then
        echo ">>> 停止并删除容器: $CONTAINERS"
        docker rm -f $CONTAINERS
    fi
    # 删除镜像
    echo ">>> 删除镜像 ${IMAGE_NAME}:latest"
    docker rmi "${IMAGE_NAME}:latest"
    echo ">>> 清理完成。"
else
    echo ">>> 镜像 ${IMAGE_NAME}:latest 不存在，跳过清理。"
fi

# Step 3: 构建镜像（使用 publish 输出目录作为构建上下文）
echo ">>> docker build..."
docker build -f Dockerfile -t "${IMAGE_NAME}:latest" .
echo ">>> docker build done."

# Step 4: 运行容器
echo ">>> docker run..."
docker run -d --name "${CONTAINER_NAME}" -p "${HOST_PORT}:${CONTAINER_PORT}" "${IMAGE_NAME}:latest"
echo ">>> 容器已启动：${CONTAINER_NAME} (端口映射 ${HOST_PORT}:${CONTAINER_PORT})"

echo ">>> 全部完成！"
