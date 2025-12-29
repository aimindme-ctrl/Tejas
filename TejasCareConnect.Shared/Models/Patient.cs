namespace TejasCareConnect.Shared.Models;

public class Patient
{
    public int Id { get; set; }
    public string MRN { get; set; } = string.Empty; // Medical Record Number
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public DateTime DateOfBirth { get; set; }
    public int Age => DateTime.Now.Year - DateOfBirth.Year - (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    public string Gender { get; set; } = string.Empty;
    public string BloodType { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string EmergencyPhone { get; set; } = string.Empty;
    public string PrimaryPhysician { get; set; } = string.Empty;
    public string InsuranceProvider { get; set; } = string.Empty;
    public string InsurancePolicyNumber { get; set; } = string.Empty;
    public string Allergies { get; set; } = string.Empty;
    public string ChronicConditions { get; set; } = string.Empty;
    public string CurrentMedications { get; set; } = string.Empty;
    public DateTime LastVisitDate { get; set; }
    public string LastVisitReason { get; set; } = string.Empty;
    public DateTime AdmissionDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string Status { get; set; } = "Active"; // Active, Discharged, Deceased
}

public class PatientListDto
{
    public int Id { get; set; }
    public string MRN { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string BloodType { get; set; } = string.Empty;
    public string PrimaryPhysician { get; set; } = string.Empty;
    public DateTime LastVisitDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PatientDetailDto
{
    public int Id { get; set; }
    public string MRN { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string BloodType { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string EmergencyPhone { get; set; } = string.Empty;
    public string PrimaryPhysician { get; set; } = string.Empty;
    public string InsuranceProvider { get; set; } = string.Empty;
    public string InsurancePolicyNumber { get; set; } = string.Empty;
    public string Allergies { get; set; } = string.Empty;
    public string ChronicConditions { get; set; } = string.Empty;
    public string CurrentMedications { get; set; } = string.Empty;
    public DateTime LastVisitDate { get; set; }
    public string LastVisitReason { get; set; } = string.Empty;
    public DateTime AdmissionDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreatePatientDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string BloodType { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = "TX";
    public string ZipCode { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string EmergencyPhone { get; set; } = string.Empty;
    public string PrimaryPhysician { get; set; } = string.Empty;
    public string InsuranceProvider { get; set; } = string.Empty;
    public string InsurancePolicyNumber { get; set; } = string.Empty;
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? CurrentMedications { get; set; }
    public string? LastVisitReason { get; set; }
}

public class UpdatePatientDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string BloodType { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string EmergencyPhone { get; set; } = string.Empty;
    public string PrimaryPhysician { get; set; } = string.Empty;
    public string InsuranceProvider { get; set; } = string.Empty;
    public string InsurancePolicyNumber { get; set; } = string.Empty;
    public string Allergies { get; set; } = string.Empty;
    public string ChronicConditions { get; set; } = string.Empty;
    public string CurrentMedications { get; set; } = string.Empty;
    public string LastVisitReason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
