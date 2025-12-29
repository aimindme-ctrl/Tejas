using TejasCareConnect.Shared.Models;
using TejasCareConnect.Web.Data;

namespace TejasCareConnect.Web.Services;

public class PatientSeeder
{
    private static readonly string[] FirstNames = new[]
    {
        "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William", "Barbara",
        "David", "Elizabeth", "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen",
        "Christopher", "Nancy", "Daniel", "Lisa", "Matthew", "Betty", "Anthony", "Margaret", "Mark", "Sandra",
        "Donald", "Ashley", "Steven", "Kimberly", "Paul", "Emily", "Andrew", "Donna", "Joshua", "Michelle",
        "Kenneth", "Dorothy", "Kevin", "Carol", "Brian", "Amanda", "George", "Melissa", "Edward", "Deborah"
    };

    private static readonly string[] LastNames = new[]
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
        "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin",
        "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson",
        "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores",
        "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts"
    };

    private static readonly string[] Cities = new[]
    {
        "Houston", "San Antonio", "Dallas", "Austin", "Fort Worth", "El Paso", "Arlington", "Corpus Christi",
        "Plano", "Lubbock", "Laredo", "Irving", "Garland", "Frisco", "McKinney", "Amarillo", "Grand Prairie"
    };

    private static readonly string[] BloodTypes = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
    
    private static readonly string[] Genders = new[] { "Male", "Female", "Other" };

    private static readonly string[] Physicians = new[]
    {
        "Dr. Sarah Johnson", "Dr. Michael Chen", "Dr. Emily Rodriguez", "Dr. James Williams", "Dr. Maria Garcia",
        "Dr. Robert Brown", "Dr. Jennifer Davis", "Dr. David Martinez", "Dr. Lisa Anderson", "Dr. Christopher Lee"
    };

    private static readonly string[] InsuranceProviders = new[]
    {
        "Blue Cross Blue Shield", "UnitedHealthcare", "Aetna", "Cigna", "Humana", "Kaiser Permanente",
        "Anthem", "Centene", "Molina Healthcare", "WellCare"
    };

    private static readonly string[] Allergies = new[]
    {
        "None", "Penicillin", "Sulfa drugs", "Aspirin", "Peanuts", "Latex", "Shellfish", "Pollen", "Dust mites"
    };

    private static readonly string[] ChronicConditions = new[]
    {
        "None", "Hypertension", "Type 2 Diabetes", "Asthma", "Arthritis", "Hyperlipidemia", "COPD", "Chronic Kidney Disease"
    };

    private static readonly string[] Medications = new[]
    {
        "None", "Lisinopril 10mg", "Metformin 500mg", "Atorvastatin 20mg", "Levothyroxine 50mcg",
        "Omeprazole 20mg", "Amlodipine 5mg", "Albuterol inhaler", "Losartan 50mg"
    };

    private static readonly string[] VisitReasons = new[]
    {
        "Annual checkup", "Follow-up appointment", "Acute illness", "Chronic disease management",
        "Preventive care", "Lab results review", "Medication refill", "Injury assessment",
        "Post-operative follow-up", "Vaccination"
    };

    private static readonly string[] Statuses = new[] { "Active", "Active", "Active", "Active", "Discharged" };

    public static List<Patient> GeneratePatients(int count)
    {
        var random = new Random(42); // Fixed seed for reproducibility
        var patients = new List<Patient>();

        for (int i = 1; i <= count; i++)
        {
            var firstName = FirstNames[random.Next(FirstNames.Length)];
            var lastName = LastNames[random.Next(LastNames.Length)];
            var gender = Genders[random.Next(Genders.Length)];
            var dateOfBirth = DateTime.Now.AddYears(-random.Next(18, 90)).AddDays(-random.Next(0, 365));
            var admissionDate = DateTime.Now.AddDays(-random.Next(0, 1095)); // Last 3 years
            var lastVisitDate = admissionDate.AddDays(random.Next(0, (DateTime.Now - admissionDate).Days));

            var patient = new Patient
            {
                MRN = $"MRN{i:D6}",
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Gender = gender,
                BloodType = BloodTypes[random.Next(BloodTypes.Length)],
                PhoneNumber = $"({random.Next(200, 999)}) {random.Next(200, 999)}-{random.Next(1000, 9999)}",
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}{random.Next(1, 999)}@email.com",
                Address = $"{random.Next(100, 9999)} {LastNames[random.Next(LastNames.Length)]} St",
                City = Cities[random.Next(Cities.Length)],
                State = "TX",
                ZipCode = $"{random.Next(75000, 79999)}",
                EmergencyContact = $"{FirstNames[random.Next(FirstNames.Length)]} {LastNames[random.Next(LastNames.Length)]}",
                EmergencyPhone = $"({random.Next(200, 999)}) {random.Next(200, 999)}-{random.Next(1000, 9999)}",
                PrimaryPhysician = Physicians[random.Next(Physicians.Length)],
                InsuranceProvider = InsuranceProviders[random.Next(InsuranceProviders.Length)],
                InsurancePolicyNumber = $"POL{random.Next(100000, 999999)}",
                Allergies = Allergies[random.Next(Allergies.Length)],
                ChronicConditions = ChronicConditions[random.Next(ChronicConditions.Length)],
                CurrentMedications = Medications[random.Next(Medications.Length)],
                LastVisitDate = lastVisitDate,
                LastVisitReason = VisitReasons[random.Next(VisitReasons.Length)],
                AdmissionDate = admissionDate,
                Status = Statuses[random.Next(Statuses.Length)],
                IsActive = true
            };

            patients.Add(patient);
        }

        return patients;
    }

    public static async Task SeedPatientsAsync(ApplicationDbContext context)
    {
        if (!context.Patients.Any())
        {
            var patients = GeneratePatients(1000);
            await context.Patients.AddRangeAsync(patients);
            await context.SaveChangesAsync();
        }
    }
}
