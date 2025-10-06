using System.Globalization;
using System.Text;
using DiskayBot.API.Contracts;
using DiskayBot.API.Contracts.Schedule;
using DiskayBot.API.Contracts.Service;

namespace DiskayBot.Bot.Messages;

public class MessageBuilder {
    public static string StartMessage() {
        return
            "*Привет, я Diskay* 💫\n\n" +
            "Твой бот-помощник в *Колледже Цифровых Технологий*.\n\n" +
            "📆 *Расписание* - */disky*\n" +
            "*-->* Покажу только то расписание, которое будет актуально для вас.";
    }

    public static string CheckBotStatus(List<PingResponse?> response) {
        string message = "Статус сервисов *Diskay*\n\n";
        
        string Emoji(string status) => status switch {
            "OK" => "✅",
            "INACTIVE" => "❌",
            _ => "❓"
        };

        foreach (var service in response) {
            message += $"**Сервис:** *{service.serviceName}* {Emoji(service.serviceStatus)} \n";
            message += $"- **База данных:** `{service.dataBaseStatus ?? "Неизвестно"}` {Emoji(service.dataBaseStatus)}";
            message += "\n\n";
        }

        return message;
    }
    
    public static string CreateAccount() {
        string result = string.Empty;

        result = "Привет!\n";
        result += "При создании профиля, выбирай *действительные данные*, они будут использоваться " +
                  "для получения персональных данных.\n";
        result += "\n";
        result += "Если ошибся, или просто хочешь изменить настройки профиля, ты всегда это можешь сделать " +
                  "воспользовавшись командой:\n" +
                  "*/settings*";
        
        return result;
    }
    public static string ShowProfile(UserData data) {
        string result = string.Empty;

        result += "𝔻𝕚𝕤𝕜𝕒𝕪 ℙ𝕣𝕠𝕗𝕚𝕝𝕖\n";
        result += '\n';
        result += $"--> `{data.username}`\n";
        result += $"Группа: *{data.group_name}* 🎓\n";
        result += '\n';
        
        return result;
    }

    public static string NotRegistered() {
        string result = "";

        result += "Кажется, вас нет в памяти *Diskay* 💫\n";
        result += '\n';
        result += "Для того чтобы создать профиль, воспользуйтесь командой: \n";
        result += "*/create_account*";
        
        return result;
    }

    public static string RegisterOffer() {
        return "Вы уверены? \nПосле согласия вы попадёте в память *Diskay* 💫\n \n(**Ваш ник и ваша группа**)";
    }

    public static string AdditionalInfoOffer() {
        return $"Отлично, вы уже можете создать профиль!\n\n" +
               $"Но не спешите!\n" +
               $"Вы можете указать *дополнительную информацию* о ваших подгруппах, " +
               $"чтобы не получать в расписании то, что вам не нужно.\n\n" +
               $"Как тебе?";
    }

    public static string ShowSchedule(DaySchedule daySchedule) {
        var sb = new StringBuilder();
        var timeNow = TimeOnly.FromDateTime(DateTime.Now);
        
        var lanchTime = false;
        var isToday = daySchedule.date == DateOnly.FromDateTime(DateTime.Now);
        
        CultureInfo russianCulture = new CultureInfo("ru-RU");
        TextInfo textInfo = russianCulture.TextInfo;
        
        string dayName = textInfo.ToTitleCase(daySchedule.date.ToString("ddd", russianCulture));
        
        if (daySchedule.date != DateOnly.FromDateTime(DateTime.Now)) {
            sb.AppendLine("Сейчас нет пар :)\nНо вот ближайшее расписание:");
            sb.AppendLine();
        }
        
        sb.AppendLine($"📅 <b>{daySchedule.date:dd.MM.yyyy}</b> {dayName} | 🫡 <b>{daySchedule.mainGroup}</b>");
        sb.AppendLine();
    
        if (daySchedule.items == null || daySchedule.items.Count == 0) {
            sb.AppendLine("🎉 <b>Пар нет! Отдыхаем!</b>");
            return sb.ToString();
        }
    
        var sortedItems = daySchedule.items.OrderBy(x => x.startTime).ToList();
    
        foreach (var item in sortedItems)
        {
            if (item.startTime > new TimeOnly(12, 0) & lanchTime != true) {
                lanchTime = true;
                sb.AppendLine("------<b>ОБЕДЕННОЕ ВРЕМЯ</b>------");
                sb.AppendLine();
            }

            if (timeNow >= new TimeOnly(12, 15) && timeNow < new TimeOnly(13, 0)) {
                
            }

            if (timeNow >= item.startTime && timeNow < item.endTime && isToday) {
                sb.AppendLine($"👉 <b>{item.startTime:HH:mm}-{item.endTime:HH:mm}</b>");
                sb.AppendLine($"Предмет: <b>{item.name}</b>");   
            }
            else if (timeNow > item.endTime && isToday) {
                sb.AppendLine($"✅ <del>{item.startTime:HH:mm}-{item.endTime:HH:mm}</del>");
                sb.AppendLine($"Предмет: <b>{item.name}</b>");   
            }
            else {
                sb.AppendLine($"--> <b>{item.startTime:HH:mm}-{item.endTime:HH:mm}</b>");
                sb.AppendLine($"Предмет: <b>{item.name}</b>");   
            }
        
            if (!string.IsNullOrEmpty(item.description))
            {
                sb.AppendLine($"Описание: <b>{item.description}</b>");
            }
        
            if (!string.IsNullOrEmpty(item.room_name) && item.subGroups == null) {
                sb.AppendLine($"Аудитория: <b>{item.room_name}</b>");
            }
            else if (item.subGroups == null) {
                sb.AppendLine($"Аудитория не указана 🤨");
            }
            
        
            if (item.subGroups != null && item.subGroups.Count > 0) {
                foreach (var subGroup in item.subGroups)
                {
                    var subInfo = $" - <code>{subGroup.subGroup}</code> : {subGroup.name}";
                    if (!string.IsNullOrEmpty(subGroup.roomName)) {
                        subInfo += $" → {subGroup.roomName}";
                    }
                    sb.AppendLine(subInfo);
                }
            }
        
            sb.AppendLine();
        }
    
        return sb.ToString();
    }

    public static string AboutBot(string version) {
        var sb = new StringBuilder();
        sb.AppendLine("*Привет, друг!* 💫\n");
        sb.AppendLine("Сейчас всё активно развивается — появляются новые идеи и изменения, и это вдохновляет.\n");
        sb.AppendLine("*Diskay* не является большим проектом, но ориентирован именно на нас с вами - обывателей колледжа.\n");
        sb.AppendLine("Огромное спасибо всем, кто поддерживает данный проект!\n");
        
        sb.AppendLine($"*Версия: {version}* ✨");
        
        return sb.ToString();
    }
}