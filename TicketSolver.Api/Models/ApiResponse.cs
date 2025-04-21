namespace TicketSolver.Api.Models;

public class BaseResponse<T>
{
    public bool Success { get; set; } // Se a operação foi bem-sucedida
    public string Message { get; set; } // Mensagem amigável ou de erro
    public T Data { get; set; } // Qualquer dado que você queira retornar
    public List<string> Errors { get; set; } // Lista de erros, se houver

    public BaseResponse(bool success, string message, T data = default, List<string> errors = null)
    {
        Success = success;
        Message = message;
        Data = data;
        Errors = errors ?? new List<string>();
    }

    public static BaseResponse<T> Ok(T data, string message = "Operação realizada com sucesso")
    {
        return new BaseResponse<T>(true, message, data);
    }

    public static BaseResponse<T> Fail(string message, List<string> errors = null)
    {
        return new BaseResponse<T>(false, message, default, errors);
    }
}

public class ApiResponse<T>(bool success, string message, T data = default, List<string> errors = null)
    : BaseResponse<T>(success, message, data, errors)
{
   
}
public class ApiResponse(bool success, string message, object data = default, List<string> errors = null)
    : BaseResponse<object>(success, message, data, errors)
{
   
}