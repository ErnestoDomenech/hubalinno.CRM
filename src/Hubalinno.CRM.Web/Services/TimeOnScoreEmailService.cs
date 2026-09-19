using System.Net.Http.Json;
using System.Text.Encodings.Web;

namespace Hubalinno.CRM.Web.Services;

public class TimeOnScoreEmailService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<TimeOnScoreEmailService> logger)
{
    public async Task<bool> SendResultAsync(
        string recipientEmail,
        string recipientName,
        string company,
        int overallScore,
        int registrationScore,
        int incidentsScore,
        int administrationScore,
        int traceabilityScore,
        int experienceScore,
        string weakestDimension,
        bool wantsContact)
    {
        var baseUrl = configuration["HubalinnoData:BaseUrl"];
        var apiKey = configuration["HubalinnoData:InternalApiKey"];

        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("Hubalinno Data email integration is not configured.");
            return false;
        }

        var client = httpClientFactory.CreateClient("HubalinnoData");
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Remove("X-Hubalinno-Internal-Key");
        client.DefaultRequestHeaders.Add("X-Hubalinno-Internal-Key", apiKey);

        var subject = $"Tu TimeOn Score: {overallScore}/100";
        var html = BuildHtml(
            recipientName,
            company,
            overallScore,
            registrationScore,
            incidentsScore,
            administrationScore,
            traceabilityScore,
            experienceScore,
            weakestDimension,
            wantsContact);

        var payload = new
        {
            to = recipientEmail,
            toName = recipientName,
            subject,
            htmlContent = html,
            plainTextContent =
                $"Tu TimeOn Score es {overallScore}/100. Principal oportunidad: {weakestDimension}.",
            source = "timeon-score",
        };

        try
        {
            using var response = await client.PostAsJsonAsync("api/internal/email", payload);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            logger.LogWarning(
                "Hubalinno Data returned {StatusCode} while sending TimeOn Score email.",
                response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not send TimeOn Score email through Hubalinno Data.");
            return false;
        }
    }

    private static string BuildHtml(
        string recipientName,
        string company,
        int overallScore,
        int registrationScore,
        int incidentsScore,
        int administrationScore,
        int traceabilityScore,
        int experienceScore,
        string weakestDimension,
        bool wantsContact)
    {
        var encoder = HtmlEncoder.Default;
        var safeName = encoder.Encode(recipientName);
        var safeCompany = encoder.Encode(company);
        var safeWeakest = encoder.Encode(weakestDimension);

        var contactText = wantsContact
            ? "<p style=\"margin:24px 0 0;color:#174d3b;\"><strong>Has solicitado que revisemos este resultado contigo.</strong> El equipo de TimeOn podrá contactar contigo para comentar las áreas detectadas.</p>"
            : "";

        return $"""
<!doctype html>
<html lang="es">
<body style="margin:0;background:#f5f5f5;font-family:Arial,Helvetica,sans-serif;color:#181818;">
  <div style="max-width:680px;margin:0 auto;padding:32px 16px;">
    <div style="background:#ffffff;border-radius:18px;padding:32px;border:1px solid #e2e2e2;">
      <div style="font-size:22px;font-weight:700;color:#07865f;">TimeOn</div>
      <p style="margin:24px 0 8px;">Hola {safeName},</p>
      <p style="margin:0 0 24px;color:#555;">Este es el resultado del diagnóstico de control horario de {safeCompany}.</p>

      <div style="background:#eefbf6;border-radius:16px;padding:24px;text-align:center;">
        <div style="font-size:14px;font-weight:700;text-transform:uppercase;letter-spacing:.08em;color:#07865f;">TimeOn Score</div>
        <div style="font-size:56px;font-weight:800;margin-top:8px;">{overallScore}<span style="font-size:22px;color:#777;">/100</span></div>
      </div>

      <h2 style="margin:32px 0 16px;">Tus cinco dimensiones</h2>
      {Metric("Registro", registrationScore)}
      {Metric("Incidencias", incidentsScore)}
      {Metric("Administración", administrationScore)}
      {Metric("Trazabilidad", traceabilityScore)}
      {Metric("Experiencia del equipo", experienceScore)}

      <div style="margin-top:28px;padding:20px;border-left:4px solid #07865f;background:#f8f8f8;">
        <div style="font-size:13px;font-weight:700;text-transform:uppercase;color:#07865f;">Principal oportunidad</div>
        <div style="font-size:22px;font-weight:700;margin-top:6px;">{safeWeakest}</div>
      </div>

      {contactText}

      <p style="margin:28px 0 0;color:#666;font-size:13px;">
        Este diagnóstico es orientativo y se basa en las respuestas facilitadas en TimeOn Score.
      </p>
    </div>
  </div>
</body>
</html>
""";
    }

    private static string Metric(string label, int score)
        => $"""
<div style="margin:0 0 14px;">
  <div style="display:flex;justify-content:space-between;margin-bottom:6px;">
    <span style="font-weight:600;">{label}</span>
    <span style="font-weight:700;">{score}/100</span>
  </div>
  <div style="height:8px;background:#e5e5e5;border-radius:999px;overflow:hidden;">
    <div style="height:8px;width:{score}%;background:#07865f;border-radius:999px;"></div>
  </div>
</div>
""";
}
