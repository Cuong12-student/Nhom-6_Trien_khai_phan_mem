# Giai đoạn 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy file .csproj và restore trước để tận dụng cache
COPY ["bhgbd/bhgbd.csproj", "bhgbd/"]
RUN dotnet restore "bhgbd/bhgbd.csproj"

# Copy toàn bộ mã nguồn và biên dịch
COPY . .
WORKDIR "/src/bhgbd"
RUN dotnet publish "bhgbd.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Giai đoạn 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Port mặc định của .NET 8 trong container là 8080
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "bhgbd.dll"]