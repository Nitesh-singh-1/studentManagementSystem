using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EmployeeManagementSystem.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"];
        }

        public async Task<(bool success, int? id, string? role, string? errorMessage)> LoginAsync(string username, string password)
        {
            var payload = new { id = "0", Username = username, Password = password, role = "" };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/LoginApi/login", content);

            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(json);
                int id = doc.RootElement.GetProperty("id").GetInt32();
                string role = doc.RootElement.GetProperty("role").GetString()!;
                return (true, id, role, null);
            }
            else
            {
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    string message = doc.RootElement.TryGetProperty("message", out var msgProp)
                        ? msgProp.GetString() ?? "Unknown error"
                        : "Unknown error";
                    return (false, null,null, message);
                }
                catch
                {
                    return (false,null, null, json);
                }
            }
        }

        public async Task<List<EmployeeResponse>?> GetAllEmployeesAsync(int? userID)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Employee/getAll?userId={userID}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var employees = JsonSerializer.Deserialize<List<EmployeeResponse>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return employees;
                }
                else
                {
                    // log or inspect response for debugging
                    var msg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {msg}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<List<EmployeeResponse>?> GetAllEmp_supervisor_Async(int? userID,int? superVisorID)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Employee/getAllEmployee?userId={userID}&superVisorID={superVisorID}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var employees = JsonSerializer.Deserialize<List<EmployeeResponse>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return employees;
                }
                else
                {
                    // log or inspect response for debugging
                    var msg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {msg}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<List<EmployeeResponse>?> getAllEmployeeForAdmin()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Employee/getAllEmployeeforAdmin");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var employees = JsonSerializer.Deserialize<List<EmployeeResponse>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return employees;
                }
                else
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {msg}");
                    return null;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }
        public async Task<EmployeeResponse> getEmployeeById(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Employee/GetEmpById/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var employees = JsonSerializer.Deserialize<EmployeeResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return employees;
                }
                else
                {
                    // log or inspect response for debugging
                    var msg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {msg}");
                    return null;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateEmployee(EmployeeResponse employee, List<IFormFile>? documents = null)
        {
            using var formData = new MultipartFormDataContent();


            //formData.Add(new StringContent(employee.modifiedBy.ToString()), "ModifiedBy");
            //formData.Add(new StringContent(employee.employeeName ?? ""), "EmployeeName");//colNo
            //formData.Add(new StringContent(employee.department ?? ""), "Department");//File No
            //formData.Add(new StringContent(employee.designation ?? ""), "Designation");//Part No
            //                                                                           // formData.Add(new StringContent(employee.age.ToString()), "Age");
            //formData.Add(new StringContent(employee.subject ?? ""), "subject");//Subject
            //                                                                   //formData.Add(new StringContent(employee.gender ?? ""), "Gender");
            //formData.Add(new StringContent(employee.address ?? ""), "Address");//from year
            //formData.Add(new StringContent(employee.ToYear ?? ""), "ToYear");// to Year
            //formData.Add(new StringContent(employee.Remarks ?? ""), "Remarks");
            formData.Add(new StringContent(employee.employeeName ?? ""), "EmployeeName");
            formData.Add(new StringContent(employee.department ?? ""), "Department");
            //formData.Add(new StringContent(employee.designation ?? ""), "Designation");
            if (!string.IsNullOrWhiteSpace(employee.designation))
            {
                formData.Add(new StringContent(employee.designation), "Designation");
            }

            formData.Add(new StringContent(employee.subject ?? ""), "Subject");
            //formData.Add(new StringContent(createdBy.ToString()), "createdBy");

            // OPTIONAL fields (only when provided)
            if (!string.IsNullOrWhiteSpace(employee.address))
            {
                formData.Add(new StringContent(employee.address), "Address");
            }

            if (!string.IsNullOrWhiteSpace(employee.ToYear))
            {
                formData.Add(new StringContent(employee.ToYear), "ToYear");
            }
            // formData.Add(new StringContent(createdBy.ToString() ?? ""), "createdBy");
            // 🧩 Add file(s)
            if (documents != null && documents.Count > 0)
            {
                foreach (var file in documents)
                {
                    var fileStream = new StreamContent(file.OpenReadStream());
                    fileStream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    formData.Add(fileStream, "Documents", file.FileName);
                }
            }

            var response = await _httpClient.PostAsync($"{_baseUrl}/Employee/edit/{employee.Id}", formData);
            return response.IsSuccessStatusCode;
        }

        public async Task<DeleteResponse> DeleteDocument(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/Employee/deleteDoc/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var deleteResponse = JsonSerializer.Deserialize<DeleteResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return deleteResponse;
                }
                else
                {
                    // log or inspect response for debugging
                    var msg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {msg}");
                    return null;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<EmployeeResponse?> AddEmployee(
    int createdBy,
    EmployeeResponse employee,
    List<IFormFile>? documents = null)
        {
            using var formData = new MultipartFormDataContent();

            // REQUIRED fields
            formData.Add(new StringContent(employee.employeeName ?? ""), "EmployeeName");
            formData.Add(new StringContent(employee.department ?? ""), "Department");
            //formData.Add(new StringContent(employee.designation ?? ""), "Designation");
            if (!string.IsNullOrWhiteSpace(employee.designation))
            {
                formData.Add(new StringContent(employee.designation), "Designation");
            }

            formData.Add(new StringContent(employee.subject ?? ""), "Subject");
            formData.Add(new StringContent(createdBy.ToString()), "createdBy");

            // OPTIONAL fields (only when provided)
            if (!string.IsNullOrWhiteSpace(employee.address))
            {
                formData.Add(new StringContent(employee.address), "Address");
            }

            if (!string.IsNullOrWhiteSpace(employee.ToYear))
            {
                formData.Add(new StringContent(employee.ToYear), "ToYear");
            }

            // Files
            if (documents != null && documents.Count > 0)
            {
                foreach (var file in documents)
                {
                    var fileStream = new StreamContent(file.OpenReadStream());
                    fileStream.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                    formData.Add(fileStream, "Documents", file.FileName);
                }
            }

            var response = await _httpClient.PostAsync($"{_baseUrl}/Employee/add", formData);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ API Error: {error}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            return new EmployeeResponse
            {
                Id = json.Value<int>("employeeId"),
                fileUniqueId = json.Value<string>("fileUniqueId"),
                employeeName = employee.employeeName,
                department = employee.department,
                designation = employee.designation,
                address = employee.address,
                ToYear = employee.ToYear,
                createdBy = createdBy
            };
        }


        public async Task<delResponse> DeleteEmp(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/Employee/delete/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var deleteResponse = JsonSerializer.Deserialize<delResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return deleteResponse;
                }
                else
                {
                    // log or inspect response for debugging
                    var msg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {msg}");
                    return null;
                }
            }
            catch(Exception ex)
            {

                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<List<UserViewModel>> GetUsersAsync(int adminId, string role)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/AdminApi/GetUsers?adminId={adminId}&role={role}");
            if (!response.IsSuccessStatusCode) return new List<UserViewModel>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<UserViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<UserViewModel>();
        }
        public async Task<object> CreateUserAsync(UserViewModel model)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/AdminApi/CreateUser", jsonContent);
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<object>(result);
        }

        public async Task<object> UpdateUserAsync(UserViewModel model)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/AdminApi/UpdateUser", jsonContent);
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<object>(result);
        }

        public async Task<object> DeleteUserAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/AdminApi/DeleteUser/{id}");
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<object>(json);
        }

        public async Task<List<UserReportViewModel>> GetUserReportAsync(string role, int? userId, DateOnly? start, DateOnly? end)
        {
            string url = $"{_baseUrl}/ReportApi/GetUserReport?role={role}&userId={userId}&startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<UserReportViewModel>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<UserReportViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<UserReportViewModel>();
        }

        public async Task<List<SupervisorReportViewModel>> GetSupervisorReportAsync(int supervisorId, DateTime? start, DateTime? end)
        {
            string url = $"{_baseUrl}/ReportApi/GetSupervisorReport/{supervisorId}?startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<SupervisorReportViewModel>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<SupervisorReportViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<SupervisorReportViewModel>();
        }
    }

}
