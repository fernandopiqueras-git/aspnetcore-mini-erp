using System.ComponentModel.DataAnnotations;
namespace MiniErp.ViewModels;
public class CreateUserViewModel { [Required,EmailAddress] public string Email {get;set;}=string.Empty; [Required,DataType(DataType.Password),StringLength(100,MinimumLength=8)] public string Password {get;set;}=string.Empty; [Required,DataType(DataType.Password),Compare(nameof(Password))] public string ConfirmPassword {get;set;}=string.Empty; [Required] public string Role {get;set;}=string.Empty; }
