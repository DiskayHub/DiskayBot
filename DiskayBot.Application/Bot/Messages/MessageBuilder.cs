using System.Text;
using DiskayBot.API.Contracts;
using DiskayBot.API.Contracts.Service;

namespace DiskayBot.Bot.Bot.Messages;

public class MessageBuilder {
    public static string StartMessage() {
        var sb = new StringBuilder();

        sb.AppendLine("<b>Привет, студент! Я Diskay</b> 💫");
        sb.AppendLine();
    
        sb.AppendLine("Моя задача — упростить тебе жизнь в мелочах, на которые ты регулярно тратишь время.");
        sb.AppendLine();
        sb.AppendLine("Я не привязан к конкретной группе и доступен каждому в <b>КЦТ</b>. ✨");
        sb.AppendLine();

        sb.AppendLine("<b>Что я предлагаю:</b>");
        sb.AppendLine(" <b>--></b> Моментальное актуальное расписание.");
        sb.AppendLine(" <b>--></b> Расписание на любой день недели.");
        sb.AppendLine(" <b>-->️</b> Стабильность: я храню данные у себя.");
        sb.AppendLine(" <b>--></b> Открытый код: буду рад вашим решениям.");
        sb.AppendLine();

        sb.AppendLine("<b>Твои команды:</b>");
        sb.Append(" <b>/disky</b>").AppendLine(" — расписание твоей группы");
        sb.Append(" <b>/check</b>").AppendLine(" — проверить другие группы");
        sb.AppendLine();

        sb.AppendLine("<b>Настройки и статус:</b>");
        sb.Append(" <b>/settings</b>").AppendLine(" — твой профиль");
        sb.Append(" <b>/show_profile</b>").AppendLine(" — настройки");
        sb.Append(" <b>/check_bot_status</b>").AppendLine(" — статус сервисов");
        sb.AppendLine(" <b>/about</b> — от разработчика (версия и changelog)");
        sb.AppendLine();

        sb.AppendLine("<i>Буду рад любой поддержке и предложениям. Расписание — это только начало!</i>🚀");
        sb.AppendLine();
        sb.AppendLine("С уважением, <b>Diskay</b>.");

        return sb.ToString();
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

    public static string ShowSchedule(DaySchedule daySchedule, bool timeDescription = true) {
        var sb = new StringBuilder();
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var timeNow = TimeOnly.FromDateTime(now);
        var isToday = daySchedule.date == today;

        string moon = (timeDescription, daySchedule.date.CompareTo(today)) switch {
            (true, > 0) => "🌅",
            (true, 0)   => "☀️",
            _           => "🌑"
        };
        sb.AppendLine($"📅 <b>{daySchedule.date:dd.MM.yyyy}</b> | 🫡 <b>{daySchedule.mainGroup}</b> | {moon}");
        sb.AppendLine("<code>——————————————————————————</code>");

        DayOfWeek[] dowOrder = [
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday
        ];
        string[] dayLabels = ["ПН", "ВТ", "СР", "ЧТ", "ПТ"];
        int MonIndex(DayOfWeek d) => d == DayOfWeek.Sunday ? 6 : (int)d - 1;
        var scheduleDow = daySchedule.date.DayOfWeek;
        var mondayOfScheduleWeek = daySchedule.date.AddDays(-MonIndex(scheduleDow));

        var weekRow = string.Join("   |   ", dayLabels.Select((label, i) => {
            var dayDate = mondayOfScheduleWeek.AddDays(i);
            string formatted = dayDate < today ? $"<s>{label}</s>" : label;
            return dowOrder[i] == scheduleDow ? $"<u>[{formatted}]</u>" : formatted;
        }));
        sb.AppendLine(weekRow);
        sb.AppendLine();

        if (daySchedule.items == null || daySchedule.items.Count == 0) {
            sb.AppendLine("🎉 <b>Пар нет! Отдыхаем!</b>");
            return sb.ToString();
        }

        var lunchTime = false;
        foreach (var item in daySchedule.items.OrderBy(x => x.startTime)) {
            if (!lunchTime && item.startTime > new TimeOnly(12, 0)) {
                lunchTime = true;
                sb.AppendLine("<code>—————————— ОБЕД ——————————</code>");
                sb.AppendLine();
            }

            bool lessonCurrent = isToday && timeNow >= item.startTime && timeNow < item.endTime;
            bool lessonDone    = daySchedule.date < today || (isToday && timeNow > item.endTime);

            string timePrefix = (lessonCurrent, lessonDone) switch {
                (true, _) => $"👉 <b>{item.startTime:HH:mm}-{item.endTime:HH:mm}</b>",
                (_, true) => $"✅ <del>{item.startTime:HH:mm}-{item.endTime:HH:mm}</del>",
                _         => $"--> <b>{item.startTime:HH:mm}-{item.endTime:HH:mm}</b>"
            };
            sb.AppendLine(timePrefix);
            sb.AppendLine($"Предмет: <b>{item.name}</b>");

            if (!string.IsNullOrEmpty(item.description))
                sb.AppendLine($"Описание: <b>{item.description}</b>");

            if (item.subGroups is { Count: > 0 }) {
                foreach (var sg in item.subGroups) {
                    var subInfo = $"  · <code>{sg.subGroup}</code> : {sg.name}";
                    if (!string.IsNullOrEmpty(sg.roomName))
                        subInfo += $" → {sg.roomName}";
                    sb.AppendLine(subInfo);
                }
            } else {
                sb.AppendLine(string.IsNullOrEmpty(item.room_name)
                    ? "Аудитория не указана 🤨"
                    : $"Аудитория: <b>{item.room_name}</b>");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string NewsPreview(string text) {
        var sb = new StringBuilder();
        sb.AppendLine("📢 <b>Предпросмотр рассылки</b>");
        sb.AppendLine();
        sb.AppendLine(text);
        sb.AppendLine();
        sb.AppendLine("<i>Разослать всем зарегистрированным пользователям?</i>");
        return sb.ToString();
    }

    public static string NewsText(string text) {
        var sb = new StringBuilder();
        sb.AppendLine("<b>Новости от Diskay</b> ✨");
        sb.AppendLine();
        sb.AppendLine(text);
        return sb.ToString();
    }

    public static string AboutBot(string version) {
        var sb = new StringBuilder();
    
        sb.AppendLine("<b>О проекте Diskay</b> 💫");
        sb.AppendLine();

        sb.AppendLine("<blockquote>\"Вы все ещё здесь?...\"</blockquote>");
        sb.AppendLine();

        sb.AppendLine("<b>Последние изменения:</b>");
        sb.AppendLine(" • Кнопки навигации в <code>/disky</code>");
        sb.AppendLine(" • Редизайн отображения пар");
        sb.AppendLine(" • Обновлённое приветствие");

        sb.AppendLine();
        // Прячем ссылку в текст «GitHub» или «Исходный код»
        sb.AppendLine("📂 <a href=\"https://github.com/DiskayHub/DiskayBot\">Исходный код проекта</a>");
        sb.AppendLine("👨‍💻 <i>Разработчик: @Laxerem</i>");
        sb.AppendLine();
        
        sb.AppendLine($"<b>Версия:</b> <code>{version}</code>"); 

        return sb.ToString();
    }
}