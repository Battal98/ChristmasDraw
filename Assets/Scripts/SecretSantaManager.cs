using UnityEngine;
using System.Net;
using System.Net.Mail;
using System.Linq;
using System.IO;
using System;

public class SecretSantaManager : MonoBehaviour
{
    [SerializeField] private ParticipantData participantsData;

    // Kullanıcı mail bilgileri
    [SerializeField] private string smtpUser = "ornek@gmail.com";
    [SerializeField] private string smtpPass = "uygulama_sifreniz";

    [SerializeField] private string mailSubject = "Yılbaşı Çekilişi Sonucu";
    [SerializeField]
    private string mailBodyTemplate = "{0} Merhaba,\n\nBu yılbaşı çekilişinde senin seçtiğin kişi: {1}.\n\nMutlu yıllar!";

    public void DoDraw()
    {
        var participants = participantsData.participants;
        if (participants == null || participants.Length == 0)
        {
            Debug.LogError("Katılımcı listesi boş veya null.");
            return;
        }

        // Domain tabanlı SMTP ayarı
        string smtpHost;
        int smtpPort = 587; // Hem Gmail hem Hotmail genelde 587 TLS portunu kullanır.

        if (smtpUser.EndsWith("@gmail.com"))
        {
            smtpHost = "smtp.gmail.com";
        }
        else if (smtpUser.EndsWith("@hotmail.com") || smtpUser.EndsWith("@outlook.com") || smtpUser.EndsWith("@live.com"))
        {
            smtpHost = "smtp-mail.outlook.com";
        }
        else
        {
            Debug.LogError("Desteklenmeyen domain. Gmail veya Hotmail/Outlook kullanın.");
            return;
        }

        int[] indices = Enumerable.Range(0, participants.Length).ToArray();
        Shuffle(indices);

        int safetyCount = 0;
        while (CheckSelfAssignment(indices, participants.Length))
        {
            Shuffle(indices);
            safetyCount++;
            if (safetyCount > 1000)
            {
                Debug.LogError("Çok fazla tekrar denemesi yapıldı. Eşleştirme algoritmasını daha iyi tasarlayın.");
                return;
            }
        }

        // Log dosyasının yolu
        string logPath = Path.Combine(Application.persistentDataPath, "secret_santa_log.txt");

        // Dosya sonuna ekleme yapmak için Append kullanıyoruz. 
        // İlk defa oluşturuluyorsa oluşturacak, varsa sonuna ekleyecek.
        using (StreamWriter writer = new StreamWriter(logPath, true))
        {
            writer.WriteLine("------ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " Çekiliş Sonuçları ------");

            for (int i = 0; i < participants.Length; i++)
            {
                string giverName = participants[i].name;
                string receiverName = participants[indices[i]].name;
                string receiverEmail = participants[indices[i]].email;

                // Log'a yaz
                writer.WriteLine(giverName + " => " + receiverName + " (" + receiverEmail + ")");

                // Mail gönder
                SendEmail(smtpHost, smtpPort, participants[i].email, mailSubject, string.Format(mailBodyTemplate, giverName, receiverName));
            }

            writer.WriteLine("--------------------------------------------------------------\n");
        }

        Debug.Log("Çekiliş tamamlandı, mailler gönderildi ve sonuçlar log dosyasına kaydedildi.\nLog dosyası: " + logPath);
    }

    private void SendEmail(string smtpHost, int smtpPort, string to, string subject, string body)
    {
        MailMessage mail = new MailMessage();
        mail.From = new MailAddress(smtpUser);
        mail.To.Add(to);
        mail.Subject = subject;
        mail.Body = body;
        mail.IsBodyHtml = false;

        SmtpClient smtpServer = new SmtpClient(smtpHost);
        smtpServer.Port = smtpPort;
        smtpServer.Credentials = new NetworkCredential(smtpUser, smtpPass) as ICredentialsByHost;
        smtpServer.EnableSsl = true;

        try
        {
            smtpServer.Send(mail);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Mail gönderilirken hata oluştu: " + ex.Message);
        }
    }

    private void Shuffle(int[] array)
    {
        System.Random rng = new System.Random();
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int temp = array[k];
            array[k] = array[n];
            array[n] = temp;
        }
    }

    private bool CheckSelfAssignment(int[] indices, int length)
    {
        for (int i = 0; i < length; i++)
        {
            if (indices[i] == i)
                return true;
        }
        return false;
    }
}
