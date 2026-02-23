# Stage 1 — build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first so Docker can cache the restore layer
COPY QuoteQuiz.sln ./
COPY QuoteQuiz.Domain/QuoteQuiz.Domain.csproj             QuoteQuiz.Domain/
COPY QuoteQuiz.Application/QuoteQuiz.Application.csproj   QuoteQuiz.Application/
COPY QuoteQuiz.Infrastructure/QuoteQuiz.Infrastructure.csproj QuoteQuiz.Infrastructure/
COPY QuoteQuiz.API/QuoteQuiz.API.csproj                   QuoteQuiz.API/
COPY QuoteQuiz.Tests/QuoteQuiz.Tests.csproj               QuoteQuiz.Tests/

RUN dotnet restore

# Copy everything and publish
COPY . .
RUN dotnet publish QuoteQuiz.API/QuoteQuiz.API.csproj -c Release -o /app/publish --no-restore

# Stage 2 — runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "QuoteQuiz.API.dll"]
