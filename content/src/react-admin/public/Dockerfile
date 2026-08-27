FROM nginx:latest

# 复制打包产物到 Nginx 默认站点目录
COPY . /usr/share/nginx/html

# 复制 Nginx 配置（已处理 Vue History 模式刷新 404 问题）
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
