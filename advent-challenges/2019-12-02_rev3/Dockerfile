FROM mcr.microsoft.com/dotnet/core/sdk:3.1.100-preview3

RUN apt-get update
RUN apt-get install -y --no-install-recommends xinetd

RUN mkdir /home/ctf && mkdir /home/ctf/build
ADD client /home/ctf/build/client
ADD server /home/ctf/build/server
ADD common /home/ctf/build/common
ADD start.sh /home/ctf/start.sh
RUN chmod +x /home/ctf/start.sh
RUN cd /home/ctf/build/client && dotnet build -c Release && cp -r bin/Release/netcoreapp3.1 /home/ctf/client && cp -r wwwroot /home/ctf/client
RUN cd /home/ctf/build/server && dotnet build -c Release && cp -r bin/Release/netcoreapp3.1 /home/ctf/server
RUN rm -rf /home/ctf/build
RUN useradd -u 8888 -m ctf
CMD ["/home/ctf/start.sh"]

