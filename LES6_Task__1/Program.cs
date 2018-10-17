Павлюков Михаил


# include <stdio.h>
# include <stdlib.h>
# include <stdbool.h>

typedef struct info_t
{
    int value;  // Значение элемента
    bool use;   // Пометка о использовании/занятости
}
TInfo;
 
//-----------------------------------------------------------------------------
// Формирует таблицу заданного размера
TInfo* GetTable(size_t size)
{
    return calloc(size, sizeof(TInfo));
}
//-----------------------------------------------------------------------------
// Вычисление стартового/первого ключа
size_t GetHashKey(size_t size, int value)
{
    return value % size;
}
//-----------------------------------------------------------------------------
// Функция получения вторичного ключа в случае коллизии
size_t GetNextKey(size_t size, int key)
{
    return ++key == size ? 0 : key;
}
//-----------------------------------------------------------------------------
// Функция добавления элемента в хеш-таблицу
bool AddToTable(TInfo table[], size_t size, int value)
{
    // Получаем первичный ключ
    int secondKey = GetHashKey(size, value);
    int baseKey = secondKey;
    bool isAdd;

    // Начиная с первичного ключа пытаемся найти свободное место
    while (table[baseKey].use
           && ((baseKey = GetNextKey(size, baseKey)) != secondKey))
    {
        ;
    }

    // Сможем добавить элемент?
    isAdd = (table[baseKey].use == false);

    // Если добавление возможно, то добавляем
    if (isAdd)
    {
        table[baseKey].value = value;
        table[baseKey].use = true;
    }

    return isAdd;
}
//-----------------------------------------------------------------------------
// Вывод на экран таблицы. В зависимости от переменной
// all выводим либо всю таблицу, либо только заполненные элементы
void PrintTable(TInfo table[], size_t size, bool all)
{
    size_t i;
    for (i = 0; i < size; ++i)
    {
        if (all || table[i].use)
        {
            printf("[%u:%d] ", i, table[i].value);
        }
    }
    printf("\n");
}
//-----------------------------------------------------------------------------
// Получаем ключ искомого элемента. Возвращает -1 в случае отрицательного
// результата. Если переменная counter задана, то в неё заносится количество
// попыток поиска элемента
int GetValue(TInfo table[], size_t size, int value, size_t* counter)
{
    // Количество попыток
    size_t count = 0;
    // Получаем первичный ключ
    int secondKey = GetHashKey(size, value);
    int baseKey = secondKey;

    // Перебираем элементы таблицы в поисках искомого значение
    while (table[baseKey].use
           && table[baseKey].value != value
           && ((baseKey = GetNextKey(size, baseKey)) != secondKey))
    {
        count++;
    }

    // Если искомый элемент был найден, то счётчик итераций
    // инкрементируем, иначе обнуляем его, ибо и так ясно,
    // что перебор осуществлялся size раз
    if ((table[baseKey].use) && (table[baseKey].value == value))
    {
        count++;
    }
    else
    {
        count = 0;
    }

    // Если нужен счётчик
    if (counter)
    {
        *counter = count;
    }

    // Возвращаем ключ в случае если счётчик не был обнулён, те
    // искомое значение было найдено, иначе возвращаем -1
    return count ? baseKey : -1;
}
//-----------------------------------------------------------------------------
// Загрузка из файла count чисел и помещение значение в table
void LoadTable(FILE* f, TInfo table[], size_t size, size_t count)
{
    int value;
    while ((count--) && (fscanf(f, "%d", &value) == 1))
    {
        if (AddToTable(table, size, value) == false)
        {
            fprintf(stderr, "error: conflict, value %d not added ...\n", value);
        }
    }
}
//-----------------------------------------------------------------------------

int main()
{
    const char CFile[] = "file.txt";

    // Количество элементов в хеш-таблице
    const size_t size = 20;
    // Создаём таблицу
    TInfo* table = GetTable(size);

    // Открываем файл
    FILE* f = fopen(CFile, "r");
    if (f == NULL)
    {
        perror(CFile);
        return EXIT_FAILURE;
    }

    // Загружаем данные из файла в таблицу
    LoadTable(f, table, size, 15);

    fclose(f);

    // Вывод используемых элементов таблицы
    printf("only used elements: ");
    PrintTable(table, size, false);
    printf("\n");

    // Вывод всей хеш-таблицы
    printf("all elements: ");
    PrintTable(table, size, true);
    printf("\n");

    int value, key;
    size_t count;

    // Просим пользователя ввести искомое значение
    printf("find value : ");
    scanf("%d", &value);

    // Если ключ был найден
    if ((key = GetValue(table, size, value, &count)) != -1)
    {
        // Выводим ключ и количество попыток
        printf("key %d, attempt %d\n", key, count);
    }
    else
    {
        // Элемент был не найден
        fprintf(stderr, "value %d not found ...\n", value);
    }

    return EXIT_SUCCESS;
}
