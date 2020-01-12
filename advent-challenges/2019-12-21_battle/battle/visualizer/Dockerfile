FROM node:11.6.0-alpine AS builder
COPY . /visualizer
WORKDIR /visualizer
RUN npm i
RUN $(npm bin)/ng build --prod

FROM nginx:1.15.8-alpine
COPY --from=builder /visualizer/dist/visualizer/ /visualizer-dist
CMD ["cp", "-r", "/visualizer-dist", "/visualizer"]
