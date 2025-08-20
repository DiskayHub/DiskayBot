using DiskayBot.API.Contracts;
using DiskayBot.API.Contracts.Service;

namespace DiskayBot.Bot.Messages;

public class MessageBuilder {
    public static string StartMessage() {
        return
            "Привет, я *Diskay* 💫\n\n" +
            "Твой бот-помощник в _Колледже Цифровых Технологий_.\n\n" +
            "Вот что я умею:\n\n" +
            "📆 *Расписание*\n" +
            "Покажу пары на сегодня, завтра или всю неделю.\n\n" +
            "⚡️ *Свободные кабинеты*\n" +
            "Найду аудитории без занятий прямо сейчас.\n\n" +
            "_(И это только начало…)_";
    }

    public static string CheckBotStatus(List<PingResponse?> response) {
        string message = "Статус сервисов *Diskay*\n\n";
        
        string Emoji(string status) => status switch
        {
            "OK" => "✅",
            "INACTIVE" => "❌",
            _ => "❓"
        };

        foreach (var service in response){
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
        return "Вы уверены? \nПосле согласия вы попадёте в память Diskay 💫\n \n(Ваш ник и ваша группа)";
    }

    public static string AdditionalInfoOffer() {
        return $"Отлично, вы уже можете создать профиль!\n\n" +
               $"Но не спешите!\n" +
               $"Вы можете указать *дополнительную информацию* о ваших подгруппах, " +
               $"чтобы не получать в расписании то, что вам не нужно.\n\n" +
               $"Как тебе?";
    }

    public static string Settings() {
        return "Настройки";
    }
}