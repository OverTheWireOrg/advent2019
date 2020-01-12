FROM mcr.microsoft.com/dotnet/core/sdk:3.1

RUN useradd -u 8888 -m ctf
RUN mkdir /home/ctf/build
ADD web /home/ctf/build/web
RUN cd /home/ctf/build/web && dotnet build -c Release && cp -r bin/Release/netcoreapp3.1 /home/ctf/web && cp -r wwwroot /home/ctf/web
RUN rm -rf /home/ctf/build
WORKDIR /home/ctf/web
USER ctf
CMD ["/home/ctf/web/web"]
