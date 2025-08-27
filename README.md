# RateLimit.Redis.Sample

This project demonstrates how to implement rate limiting using Redis in an ASP.NET Core application.

## Features

- Fixed window rate limiting for API endpoints
- User authentication
- Flexible configuration via `appsettings.json`
- Redis as the backend for rate limit storage

## Project Structure

- `Program.cs`: Application entry point
- `Controllers/`
  - `AuthController.cs`: Authentication endpoints
  - `SampleController.cs`: Example endpoints protected by rate limiting
- `Attributes/`
  - `EnsureRateLimitTokenAttribute.cs`: Ensures the presence of a rate limit token
  - `FixedWindowRateLimitAttribute.cs`: Implements fixed window rate limiting
- `Config/`
  - `CookieConfig.cs`, `RateLimitConfig.cs`, `RateLimitPolicyConfig.cs`, `RedisConfig.cs`: Configuration classes
- `Extensions/RateLimitExtensions.cs`: Extension methods for rate limiting setup
- `Services/HashToken/HashTokenService.cs`: Service for generating and validating tokens
- `appsettings.json`: Application configuration

## How to Run

1. **Prerequisites:**  
   - .NET 8.0 SDK or later  
   - A running Redis instance

2. **Configuration:**  
   Update the Redis connection settings in `appsettings.json` as needed.

3. **Build and Run:**
   ```sh
   dotnet build
   dotnet run
   ```

4. **Testing Endpoints:**  
   Use tools like Postman or curl to test the endpoints defined in the controllers.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE)