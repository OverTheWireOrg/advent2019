FROM mcr.microsoft.com/dotnet/core/sdk:3.0

RUN apt-get update
RUN apt-get install -y --no-install-recommends xinetd

RUN mkdir /home/ctf && mkdir /home/ctf/build
ADD pwn2.csproj /home/ctf/build
ADD Program.cs /home/ctf/build
ADD flag.txt /home/ctf
RUN cd /home/ctf/build && dotnet build -c Release && cp bin/Release/netcoreapp3.0/* ../ && cd .. && rm -rf build && chmod -R 755 .
RUN useradd -u 8888 -m ctf
CMD ["/usr/sbin/xinetd", "-dontfork"]

