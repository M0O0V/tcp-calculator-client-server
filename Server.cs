using System;
using System.Net;               // Подключение  для работы с IP-адресами и сетевыми интерфейсами
using System.Net.Sockets;       // Подключение  для работы с сокетами
using System.Text;              // Подключение  для работы с кодировками и для преобразования строк

class Server
{
    // Счетчик клиентов для присваивания уникальных номеров
    private static int clientCounter = 0;

    static void Main(string[] args)
    {
        // Настройка сервера
        var serverAddress = "127.0.0.1"; // Указание IP-адреса сервера (127.0.0.1 — это локальный адрес)
        var port = 9999; // Порт для подключения клиентов
        var listener = new TcpListener(IPAddress.Parse(serverAddress), port); // Создание слушателя TCP-сокета на указанном IP и порту

        listener.Start(); // Запуск сервера на прослушивание входящих соединений
        Console.WriteLine($"Сервер запущен на {serverAddress}:{port}. Ожидание подключения клиентов..."); // Сообщение о запуске сервера
        Console.WriteLine("------------------------------------------"); // Разделитель для логов
        Console.WriteLine(); // Пустая строка для удобства

        while (true)
        {
            // Принимаем новое подключение от клиента
            var clientSocket = listener.AcceptSocket(); // Ожидание подключения клиента и принятие сокета клиента

            // Создаем новый поток для обработки клиента
            Thread clientThread = new Thread(HandleClient); // Новый поток для каждого клиента
            clientThread.Start(clientSocket); // Запуск потока для обработки запроса клиента
        }
    }

    // Метод для обработки запросов от клиента
    private static void HandleClient(object obj)
    {
        var clientSocket = (Socket)obj; // Преобразуем переданный объект в сокет клиента
        var stream = new NetworkStream(clientSocket); // Создаем поток для чтения и записи данных в сеть

        int clientId = Interlocked.Increment(ref clientCounter);  // Получаем уникальный номер для каждого клиента (потокобезопасное увеличение)

        try
        {
            // Выводим сообщение о подключении клиента с уникальным номером
            Console.WriteLine($"-------->  Клиент_{clientId} подключился.");
            Console.WriteLine(); // Пустая строка для удобства

            while (true)
            {
                // Чтение данных от клиента
                byte[] buffer = new byte[1024]; // Буфер для хранения данных от клиента
                int bytesRead = stream.Read(buffer, 0, buffer.Length); // Чтение данных из потока

                if (bytesRead == 0)
                {
                    break; // Прерывание, если клиент закрыл соединение
                }
                Console.WriteLine("--------------------------------------------------------------------------------"); // Разделитель для логов
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead); // Преобразуем прочитанные данные в строку
                Console.WriteLine($"Запрос от клиента_{clientId}: {request}"); // Выводим запрос клиента на сервере

                // Разбор запроса (делим строку на три части: число, операция, число)
                string[] parts = request.Split(' ');
                if (parts.Length != 3) // Проверка на корректный формат запроса (должно быть три части)
                {
                    SendResponse(stream, "Неверный формат запроса."); // Отправка ошибки в случае неверного формата
                    continue; // Переход к следующей итерации
                }

                if (!double.TryParse(parts[0], out double operand1) || !double.TryParse(parts[2], out double operand2))
                {
                    SendResponse(stream, "Неверные числа."); // Отправка ошибки, если числа не могут быть преобразованы
                    continue; // Переход к следующей итерации
                }

                string operation = parts[1]; // Операция (например, +, -, *, /)
                double result = 0; // Переменная для хранения результата
                string responseMessage = ""; // Сообщение, которое будет отправлено клиенту
                string operationName = ""; // Для отображения в логах типа операции

                // Обработка различных операций
                switch (operation)
                {
                    case "+":
                        result = operand1 + operand2; // Выполнение операции сложения
                        responseMessage = $"{operand1}+{operand2}={result}"; // Формирование сообщения с результатом
                        operationName = "Сложение (+)";
                        break;
                    case "-":
                        result = operand1 - operand2; // Вычитание
                        responseMessage = $"{operand1}-{operand2}={result}"; // Формирование сообщения с результатом
                        operationName = "Вычитание (-)";
                        break;
                    case "*":
                        result = operand1 * operand2; // Умножение
                        responseMessage = $"{operand1}*{operand2}={result}"; // Формирование сообщения с результатом
                        operationName = "Умножение (*)";
                        break;
                    case "/":
                        if (operand2 == 0) // Проверка деления на ноль
                        {
                            responseMessage = "Ошибка: деление на ноль!";
                        }
                        else
                        {
                            result = operand1 / operand2; // Деление
                            responseMessage = $"{operand1}/{operand2}={result}"; // Формирование сообщения с результатом
                        }
                        operationName = "Деление (/)";
                        break;
                    default:
                        responseMessage = "Неизвестная операция."; // Если операция не распознана
                        break;
                }

                // Вывод на сервере в нужном формате
                Console.WriteLine($"Клиент_{clientId} запрашивает операцию {operationName} с числами \"{operand1}\" и \"{operand2}\". Результат \"{result}\"");
                Console.WriteLine("--------------------------------------------------------------------------------"); // Разделитель для логов

                // Отправка ответа клиенту
                SendResponse(stream, responseMessage); // Отправляем результат обратно клиенту
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке запроса от клиента_{clientId}: {ex.Message}"); // Обработка ошибок при запросах
        }
        finally
        {
            // Закрытие соединения с клиентом
            clientSocket.Close(); // Закрытие сокета
            Console.WriteLine($"<--------  Клиент_{clientId} отключился."); // Сообщение об отключении клиента
            Console.WriteLine(); // Пустая строка для удобства
        }
    }

    // Метод для отправки ответа клиенту
    private static void SendResponse(NetworkStream stream, string message)
    {
        byte[] responseBytes = Encoding.UTF8.GetBytes(message); // Преобразуем сообщение в байты
        stream.Write(responseBytes, 0, responseBytes.Length); // Отправляем байты через поток
    }
}
