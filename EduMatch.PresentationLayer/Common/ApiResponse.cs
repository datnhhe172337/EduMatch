using EduMatch.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace EduMatch.PresentationLayer.Common
{

	public class ApiResponse<T>
	{
		[JsonPropertyName("success")]
		public bool Success { get; set; }

		[JsonPropertyName("message")]
		public string Message { get; set; } = string.Empty;

		[JsonPropertyName("data")]
		public T? Data { get; set; }

		[JsonPropertyName("error")]
		public object? Error { get; set; }

		private ApiResponse() { }

		public static ApiResponse<T> Ok(T? data, string? message = null)
			=> new()
			{
				Success = true,
				Message = message ?? "Success",
				Data = data,
				Error = null
			};

		
		public static ApiResponse<T> Fail(string message, object? error = null)
			=> new()
			{
				Success = false,
				Message = message,
				Data = default,
				Error = error
			};
    }

	
}

