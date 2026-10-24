namespace Backend.Application.Users.DTOs;

public class AuthResultDto
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? Role { get; set; }
}