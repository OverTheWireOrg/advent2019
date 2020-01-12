FROM mcr.microsoft.com/dotnet/core/sdk:3.1

RUN apt-get update && apt-get install -y --no-install-recommends gcc libc6-dev libseccomp-dev

RUN useradd -u 8888 -m ctf
RUN mkdir /home/ctf/build
ADD framework /home/ctf/build/framework
ADD simulator /home/ctf/build/simulator
ADD common /home/ctf/build/common
ADD sandbox /home/ctf/build/sandbox
RUN cd /home/ctf/build/sandbox && gcc -lseccomp -o sandbox sandbox.c && cp sandbox /bin
RUN cd /home/ctf/build/framework && dotnet build -c Release && cp -r bin/Release/netcoreapp3.1 /home/ctf/framework
RUN rm -rf /home/ctf/build
USER ctf
CMD ["/home/ctf/framework/framework", "/data/binaries", "/data/results", "/bin/sandbox", "6"]
