using System.ComponentModel.DataAnnotations;
namespace MiniErp.ViewModels;
public class ChangePasswordViewModel { [Required,DataType(DataType.Password)] public string CurrentPassword {get;set;}=string.Empty; [Required,DataType(DataType.Password),StringLength(100,MinimumLength=8)] public string NewPassword {get;set;}=string.Empty; [Required,DataType(DataType.Password),Compare(nameof(NewPassword))] public string ConfirmPassword {get;set;}=string.Empty; }
