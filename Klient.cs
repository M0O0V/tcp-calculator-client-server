using System; // Подключение пространства имен для базовых классов, таких как Console, Exception и другие.
using System.Net.Sockets; // Подключение пространства имен для работы с сокетами, включая TcpClient и NetworkStream.
using System.Text; // Подключение пространства имен для работы с кодировками, например, для преобразования строк в байты и обратно.

class Klient
{
    // Основная функция для запуска клиента
    static void Main(string[] args)
    {
        // Указываем IP-адрес сервера (локальный адрес)
        var serverAddress = "192.168.0.21"; // Адрес сервера (локальный)
        var port = 9999; // Порт для подключения клиента к серверу

        // Создаем объект клиента и подключаемся к серверу на указанном IP и порту
        var clientSocket = new TcpClient(serverAddress, port);
        var stream = clientSocket.GetStream(); // Получаем поток для передачи и получения данных

        // Выводим сообщение, что клиент подключен к серверу
        Console.WriteLine("Подключено к серверу. Для завершения работы введите 'exit'.");

        while (true)
        {
            // Отображаем меню с возможными операциями
            Console.WriteLine("----------------------------+");
            Console.WriteLine("Выберите операцию:          |");
            Console.WriteLine("1. Сложение (+)             |");
            Console.WriteLine("2. Вычитание (-)            |");
            Console.WriteLine("3. Умножение (*)            |");
            Console.WriteLine("4. Деление (/)              |");
            Console.WriteLine("Для выхода наберите 'exit'  |");
            Console.WriteLine("----------------------------+");

            // Ввод номера операции
            Console.Write("Номер операции:  ");
            string operation = Console.ReadLine(); // Получаем строку с выбором операции от пользователя

            // Если введено 'exit', завершаем работу клиента
            if (operation.ToLower() == "exit")
            {
                break; // Выход из цикла и завершение программы
            }

            // Конвертация номера в соответствующую операцию
            switch (operation) // В зависимости от выбора пользователя, задаем символ операции
            {
                case "1":
                    operation = "+"; // Если выбрано 1, операция - сложение
                    break;
                case "2":
                    operation = "-"; // Если выбрано 2, операция - вычитание
                    break;
                case "3":
                    operation = "*"; // Если выбрано 3, операция - умножение
                    break;
                case "4":
                    operation = "/"; // Если выбрано 4, операция - деление
                    break;
                default:
                    Console.WriteLine("Выберите корректную операцию."); // Если введен неверный номер, выводим ошибку
                    continue; // Переход к следующей итерации цикла
            }

            // Ввод первого числа
            double operand1; // Переменная для первого числа
            Console.Write("Введите первое число: ");
            // Проверка, что введено правильное число
            while (!double.TryParse(Console.ReadLine(), out operand1))
            {
                Console.WriteLine("Введите пожалуйста цифры."); // Если введено некорректное значение, просим ввести заново
                Console.Write("Введите первое число: ");
            }

            // Ввод второго числа
            double operand2; // Переменная для второго числа
            Console.Write("Введите второе число: ");
            // Проверка, что введено правильное число
            while (!double.TryParse(Console.ReadLine(), out operand2))
            {
                Console.WriteLine("Введите пожалуйста цифры."); // Если введено некорректное значение, просим ввести заново
                Console.Write("Введите второе число: ");
            }

            // Формируем строку запроса, объединяя оба числа и операцию
            string request = $"{operand1} {operation} {operand2}"; // Строка запроса, которая будет отправлена серверу

            // Преобразуем строку запроса в массив байт и отправляем серверу
            byte[] messageBytes = Encoding.UTF8.GetBytes(request); // Преобразуем строку в массив байт
            stream.Write(messageBytes, 0, messageBytes.Length); // Отправка данных через поток

            // Получаем ответ от сервера
            byte[] buffer = new byte[1024]; // Буфер для получения данных от сервера
            int bytesRead = stream.Read(buffer, 0, buffer.Length); // Чтение данных в буфер
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead); // Преобразуем полученные байты обратно в строку
            Console.WriteLine($"------> Ответ от сервера: {response}"); // Выводим полученный от сервера ответ
            Console.WriteLine(""); // Пустая строка для удобства
        }

        // Закрытие соединения с сервером
        clientSocket.Close(); // Закрываем сокет после завершения работы клиента
        Console.WriteLine("Соединение с сервером закрыто."); // Сообщение о закрытии соединения
    }
}
