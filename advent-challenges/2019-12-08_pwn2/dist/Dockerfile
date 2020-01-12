# NOTE: This Dockerfile is provided for reference ONLY.
# It is NOT the production Dockerfile used for the challenge.
# The sole purpose here is to reveal the system environment
# that the challenge is being hosted in.
#
# In other words the most important clause is the FROM clause.
FROM mcr.microsoft.com/dotnet/core/sdk:3.0

RUN useradd -u 1234 -m demo
ADD pwn2.csproj /home/demo
ADD Program.cs /home/demo
ADD flag.txt /home/demo
RUN cd /home/demo && dotnet build -c Release
WORKDIR /home/demo
CMD ["bin/Release/netcoreapp3.0/pwn2"]
