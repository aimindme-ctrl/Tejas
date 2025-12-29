using System.Net.Http.Json;
using TejasCareConnect.Shared.Models;

namespace TejasCareConnect.Shared.Services;

public interface IPatientService
{
    Task<ApiResponse<List<PatientListDto>>> GetPatientsAsync(int page = 1, int pageSize = 50, string? search = null, string? status = null);
    Task<ApiResponse<PatientDetailDto>> GetPatientByIdAsync(int patientId);
    Task<ApiResponse<PatientDetailDto>> CreatePatientAsync(CreatePatientDto createDto);
    Task<ApiResponse<PatientDetailDto>> UpdatePatientAsync(int patientId, UpdatePatientDto updateDto);
}

public class PatientService : IPatientService
{
    private readonly HttpClient _httpClient;

    public PatientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<List<PatientListDto>>> GetPatientsAsync(int page = 1, int pageSize = 50, string? search = null, string? status = null)
    {
        try
        {
            var url = $"api/patients?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            if (!string.IsNullOrEmpty(status))
                url += $"&status={Uri.EscapeDataString(status)}";

            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<PatientListDto>>>(url);
            return response ?? new ApiResponse<List<PatientListDto>> { Success = false, Message = "Failed to retrieve patients" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<PatientListDto>> { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse<PatientDetailDto>> GetPatientByIdAsync(int patientId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<PatientDetailDto>>($"api/patients/{patientId}");
            return response ?? new ApiResponse<PatientDetailDto> { Success = false, Message = "Failed to retrieve patient" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PatientDetailDto> { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse<PatientDetailDto>> CreatePatientAsync(CreatePatientDto createDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/patients", createDto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PatientDetailDto>>();
            return result ?? new ApiResponse<PatientDetailDto> { Success = false, Message = "Failed to create patient" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PatientDetailDto> { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse<PatientDetailDto>> UpdatePatientAsync(int patientId, UpdatePatientDto updateDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/patients/{patientId}", updateDto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PatientDetailDto>>();
            return result ?? new ApiResponse<PatientDetailDto> { Success = false, Message = "Failed to update patient" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PatientDetailDto> { Success = false, Message = $"Error: {ex.Message}" };
        }
    }
}
