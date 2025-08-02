using DiskayBot.API.Contracts;
using Telegram.Bot.Requests;

namespace DiskayBot.Bot.Abstractions;

public class MessageBuilder {
    public static string StartMessage() {
        string result = string.Empty;

        result += "Привет, я Diskay 💫\n";
        result += "\n";
        result += "Персональный бот-помощник *Колледжа Цифровых Технологий*.\n";
        result += "\n";
        result += "Я разрабатываюсь для того, чтобы упростить получение, взаимодействие с данными колледжа.\n";
        result += "Функционал, который тебе доступен:\n";
        result += "\n";
        result += "📆\nПолучение актуального расписания на неделю или на день.\n";
        result += "⚡️\nПоиск свободных кабинетов.";
        
        return result;
    }
    
    public static string CreateAccount() {
        string result = string.Empty;

        result = "Привет!\n";
        result += "При создании профиля, выбирай *действительные данные*, они будут использоваться " +
                  "для получения персонального расписания.\n";
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
}