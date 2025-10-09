# 1) Runtime image (chạy app)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 3002

# Kestrel lắng nghe mọi interface, cổng 3002
ENV ASPNETCORE_URLS=http://0.0.0.0:3002

# 2) SDK image (build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file .csproj để tận dụng cache restore
COPY ["EduMatch.PresentationLayer/EduMatch.PresentationLayer.csproj", "EduMatch.PresentationLayer/"]
COPY ["EduMatch.BusinessLogicLayer/EduMatch.BusinessLogicLayer.csproj", "EduMatch.BusinessLogicLayer/"]
COPY ["EduMatch.DataAccessLayer/EduMatch.DataAccessLayer.csproj", "EduMatch.DataAccessLayer/"]

# Restore dependency cho project startup
RUN dotnet restore "EduMatch.PresentationLayer/EduMatch.PresentationLayer.csproj"

# Copy toàn bộ source
COPY . .

# Build
WORKDIR "/src/EduMatch.PresentationLayer"
RUN dotnet build "EduMatch.PresentationLayer.csproj" -c Release -o /app/build

# 3) Publish
FROM build AS publish
RUN dotnet publish "EduMatch.PresentationLayer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4) Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EduMatch.PresentationLayer.dll"]




# docker run -p 3002:3002 --env-file EduMatch.PresentationLayer/.env datnhm5/edumatch-api:v0