using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TejasCareConnect.Shared.Models;
using TejasCareConnect.Web.Data;

namespace TejasCareConnect.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PatientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PatientListDto>>>> GetPatients(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        try
        {
            var query = _context.Patients.AsQueryable();

            // If user is a Viewer with PatientId, only show their own record
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var patientIdClaim = User.FindFirst("PatientId")?.Value;
            
            if (userRole == "Viewer" && !string.IsNullOrEmpty(patientIdClaim) && int.TryParse(patientIdClaim, out int patientId))
            {
                query = query.Where(p => p.Id == patientId);
            }

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.FirstName.Contains(search) ||
                    p.LastName.Contains(search) ||
                    p.MRN.Contains(search) ||
                    p.Email.Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            var totalCount = await query.CountAsync();
            
            var patients = await query
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PatientListDto
                {
                    Id = p.Id,
                    MRN = p.MRN,
                    FullName = p.FirstName + " " + p.LastName,
                    Age = DateTime.Now.Year - p.DateOfBirth.Year,
                    Gender = p.Gender,
                    BloodType = p.BloodType,
                    PrimaryPhysician = p.PrimaryPhysician,
                    LastVisitDate = p.LastVisitDate,
                    Status = p.Status
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<PatientListDto>>
            {
                Success = true,
                Message = $"Retrieved {patients.Count} of {totalCount} patients",
                Data = patients
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<List<PatientListDto>>
            {
                Success = false,
                Message = $"Error retrieving patients: {ex.Message}"
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PatientDetailDto>>> GetPatient(int id)
    {
        try
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
            {
                return NotFound(new ApiResponse<PatientDetailDto>
                {
                    Success = false,
                    Message = "Patient not found"
                });
            }

            // If user is a Viewer with PatientId, only allow viewing their own record
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var patientIdClaim = User.FindFirst("PatientId")?.Value;
            
            if (userRole == "Viewer" && !string.IsNullOrEmpty(patientIdClaim) && int.TryParse(patientIdClaim, out int userPatientId))
            {
                if (patient.Id != userPatientId)
                {
                    return Forbid();
                }
            }

            var patientDto = new PatientDetailDto
            {
                Id = patient.Id,
                MRN = patient.MRN,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                FullName = patient.FirstName + " " + patient.LastName,
                Age = DateTime.Now.Year - patient.DateOfBirth.Year,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                BloodType = patient.BloodType,
                PhoneNumber = patient.PhoneNumber,
                Email = patient.Email,
                Address = patient.Address,
                City = patient.City,
                State = patient.State,
                ZipCode = patient.ZipCode,
                EmergencyContact = patient.EmergencyContact,
                EmergencyPhone = patient.EmergencyPhone,
                PrimaryPhysician = patient.PrimaryPhysician,
                InsuranceProvider = patient.InsuranceProvider,
                InsurancePolicyNumber = patient.InsurancePolicyNumber,
                Allergies = patient.Allergies,
                ChronicConditions = patient.ChronicConditions,
                CurrentMedications = patient.CurrentMedications,
                LastVisitDate = patient.LastVisitDate,
                LastVisitReason = patient.LastVisitReason,
                AdmissionDate = patient.AdmissionDate,
                Status = patient.Status
            };

            return Ok(new ApiResponse<PatientDetailDto>
            {
                Success = true,
                Message = "Patient retrieved successfully",
                Data = patientDto
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<PatientDetailDto>
            {
                Success = false,
                Message = $"Error retrieving patient: {ex.Message}"
            });
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetPatientStats()
    {
        try
        {
            var stats = new
            {
                TotalPatients = await _context.Patients.CountAsync(),
                ActivePatients = await _context.Patients.CountAsync(p => p.Status == "Active"),
                DischargedPatients = await _context.Patients.CountAsync(p => p.Status == "Discharged"),
                AverageAge = await _context.Patients.AverageAsync(p => DateTime.Now.Year - p.DateOfBirth.Year),
                GenderDistribution = await _context.Patients
                    .GroupBy(p => p.Gender)
                    .Select(g => new { Gender = g.Key, Count = g.Count() })
                    .ToListAsync(),
                BloodTypeDistribution = await _context.Patients
                    .GroupBy(p => p.BloodType)
                    .Select(g => new { BloodType = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync()
            };

            return Ok(new { Success = true, Data = stats });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = $"Error retrieving stats: {ex.Message}" });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Contributor")]
    public async Task<ActionResult<ApiResponse<PatientDetailDto>>> CreatePatient([FromBody] CreatePatientDto createDto)
    {
        try
        {
            // Generate unique MRN
            var lastPatient = await _context.Patients.OrderByDescending(p => p.Id).FirstOrDefaultAsync();
            var mrnNumber = lastPatient != null ? lastPatient.Id + 1001 : 1001;
            var mrn = $"MRN{mrnNumber:D6}";

            var patient = new Patient
            {
                MRN = mrn,
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                DateOfBirth = createDto.DateOfBirth,
                Gender = createDto.Gender,
                BloodType = createDto.BloodType,
                PhoneNumber = createDto.PhoneNumber,
                Email = createDto.Email,
                Address = createDto.Address,
                City = createDto.City,
                State = createDto.State,
                ZipCode = createDto.ZipCode,
                EmergencyContact = createDto.EmergencyContact,
                EmergencyPhone = createDto.EmergencyPhone,
                PrimaryPhysician = createDto.PrimaryPhysician,
                InsuranceProvider = createDto.InsuranceProvider,
                InsurancePolicyNumber = createDto.InsurancePolicyNumber,
                Allergies = createDto.Allergies ?? "None",
                ChronicConditions = createDto.ChronicConditions ?? "None",
                CurrentMedications = createDto.CurrentMedications ?? "None",
                LastVisitDate = DateTime.Now,
                LastVisitReason = createDto.LastVisitReason ?? "Initial Registration",
                AdmissionDate = DateTime.Now,
                IsActive = true,
                Status = "Active"
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            var patientDto = new PatientDetailDto
            {
                Id = patient.Id,
                MRN = patient.MRN,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                FullName = patient.FirstName + " " + patient.LastName,
                Age = DateTime.Now.Year - patient.DateOfBirth.Year,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                BloodType = patient.BloodType,
                PhoneNumber = patient.PhoneNumber,
                Email = patient.Email,
                Address = patient.Address,
                City = patient.City,
                State = patient.State,
                ZipCode = patient.ZipCode,
                EmergencyContact = patient.EmergencyContact,
                EmergencyPhone = patient.EmergencyPhone,
                PrimaryPhysician = patient.PrimaryPhysician,
                InsuranceProvider = patient.InsuranceProvider,
                InsurancePolicyNumber = patient.InsurancePolicyNumber,
                Allergies = patient.Allergies,
                ChronicConditions = patient.ChronicConditions,
                CurrentMedications = patient.CurrentMedications,
                LastVisitDate = patient.LastVisitDate,
                LastVisitReason = patient.LastVisitReason,
                AdmissionDate = patient.AdmissionDate,
                Status = patient.Status
            };

            return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, new ApiResponse<PatientDetailDto>
            {
                Success = true,
                Message = $"Patient created successfully with MRN: {patient.MRN}",
                Data = patientDto
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<PatientDetailDto>
            {
                Success = false,
                Message = $"Error creating patient: {ex.Message}"
            });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PatientDetailDto>>> UpdatePatient(int id, [FromBody] UpdatePatientDto updateDto)
    {
        try
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
            {
                return NotFound(new ApiResponse<PatientDetailDto>
                {
                    Success = false,
                    Message = "Patient not found"
                });
            }

            // Update patient fields
            patient.FirstName = updateDto.FirstName;
            patient.LastName = updateDto.LastName;
            patient.DateOfBirth = updateDto.DateOfBirth;
            patient.Gender = updateDto.Gender;
            patient.BloodType = updateDto.BloodType;
            patient.PhoneNumber = updateDto.PhoneNumber;
            patient.Email = updateDto.Email;
            patient.Address = updateDto.Address;
            patient.City = updateDto.City;
            patient.State = updateDto.State;
            patient.ZipCode = updateDto.ZipCode;
            patient.EmergencyContact = updateDto.EmergencyContact;
            patient.EmergencyPhone = updateDto.EmergencyPhone;
            patient.PrimaryPhysician = updateDto.PrimaryPhysician;
            patient.InsuranceProvider = updateDto.InsuranceProvider;
            patient.InsurancePolicyNumber = updateDto.InsurancePolicyNumber;
            patient.Allergies = updateDto.Allergies;
            patient.ChronicConditions = updateDto.ChronicConditions;
            patient.CurrentMedications = updateDto.CurrentMedications;
            patient.LastVisitReason = updateDto.LastVisitReason;
            patient.Status = updateDto.Status;

            await _context.SaveChangesAsync();

            var patientDto = new PatientDetailDto
            {
                Id = patient.Id,
                MRN = patient.MRN,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                FullName = patient.FirstName + " " + patient.LastName,
                Age = DateTime.Now.Year - patient.DateOfBirth.Year,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                BloodType = patient.BloodType,
                PhoneNumber = patient.PhoneNumber,
                Email = patient.Email,
                Address = patient.Address,
                City = patient.City,
                State = patient.State,
                ZipCode = patient.ZipCode,
                EmergencyContact = patient.EmergencyContact,
                EmergencyPhone = patient.EmergencyPhone,
                PrimaryPhysician = patient.PrimaryPhysician,
                InsuranceProvider = patient.InsuranceProvider,
                InsurancePolicyNumber = patient.InsurancePolicyNumber,
                Allergies = patient.Allergies,
                ChronicConditions = patient.ChronicConditions,
                CurrentMedications = patient.CurrentMedications,
                LastVisitDate = patient.LastVisitDate,
                LastVisitReason = patient.LastVisitReason,
                AdmissionDate = patient.AdmissionDate,
                Status = patient.Status
            };

            return Ok(new ApiResponse<PatientDetailDto>
            {
                Success = true,
                Message = "Patient updated successfully",
                Data = patientDto
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<PatientDetailDto>
            {
                Success = false,
                Message = $"Error updating patient: {ex.Message}"
            });
        }
    }
}
