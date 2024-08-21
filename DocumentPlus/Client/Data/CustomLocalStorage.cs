using Blazored.LocalStorage;
using System.Reflection;


namespace DocumentPlus.Client.Data
{
    // Класс необходимый для работы с локальным хранилищем браузера
    // Работа основа на добавление объектов ДТО как пользователь и документ
    // Хотя в теории он может обрабатывать и другие классы но надо тестировать
    public class CustomLocalStorage
    {
        private readonly ILocalStorageService storage;

        public CustomLocalStorage(ILocalStorageService storage)
        {
            this.storage = storage;
        }

        // Добавляет информацию на основе класса. Пример формата "UserInfo:Id" со значением "22"
        public async Task SetDataAsync<T>(T data)
        {
            PropertyInfo[]? properties = data.GetType()
            .GetProperties();

            Type type = data.GetType();

            foreach (PropertyInfo field in properties)
            {
                await storage.SetItemAsStringAsync(type.Name + ":" + field.Name, field.GetValue(data).ToString());
            }
        }

        // Добавляет инфомрацюи на основе ключ-значения. Пример "Id" со значением \"22"
        public async Task SetDataAsync<T>(string key, T value)
        {
            await storage.SetItemAsStringAsync(key, value.ToString());
        }


        // Я не гарантирую что это исчадие сатаны будет работать
        // Каждое поле класса должно реализовывать IConvertible чтобы оно могло работать
        // Иначе, скорее всего, оно выдаст исключение что не гуд
        // А ну ещё и оно должно уметь конвертить своё значение со строки
        // Ввиду того что всё представлено в таком виде
        public async Task<T> GetValueAsync<T>()
        {
            Type type = typeof(T);

            PropertyInfo[] properties = type.GetProperties();

            T? result = Activator.CreateInstance<T>();

            foreach (PropertyInfo field in properties)
            {
                Type? selectedType = field.PropertyType;
                string selectedKey = type.Name + ":" + field.Name;
                string? selectedValue = await storage.GetItemAsStringAsync(selectedKey);


                field.SetValue(result, Convert.ChangeType(selectedValue, selectedType));
            }

            return result;
        }

        public async Task<T> GetValueAsync<T>(string key)
        {
            Type type = typeof(T);
            string? storageValue = await storage.GetItemAsStringAsync(key);

            return (T)Convert.ChangeType(storageValue, type);
        }


        // Удаляет информацию на основе класса. Пример формата "UserInfo:Id"
        public async Task DeleteDataAsync<T>()
        {
            Type type = typeof(T);
            PropertyInfo[]? properties = type.GetProperties();

            foreach (PropertyInfo field in properties)
            {
                await storage.RemoveItemAsync(type.Name);
            }

        }

        // Удаляет информацию на основе ключа. Пример формата "Id"
        public async Task DeleteDataAsync(string key)
        {
            await storage.RemoveItemAsync(key);
        }

        // Удаляет ВСЮ информацию в хранилище
        public async Task ClearAsync()
        {
            await storage.ClearAsync();
        }
    }
}
