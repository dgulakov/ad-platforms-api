Clone the repository and open AdPlatformsApi.sln file in Visual Studio 2022 or later, you should have .NET 9.0 or later to build and launch the service. In postman folder you can find a collection of API calls which can be imported to <a href="https://www.postman.com/downloads/">Postman</a> application or you can use your favorite tool similar to curl to make calls to API.  

API has next endpoints:  
* GET /adplatforms - returns all platforms and its details existing in memory of service at this moment
* GET /adplatforms/search/{*location} - returns names of all platforms suitable for provided location (location is optional). In case if location doesn't provided, names of all platforms will be returned. Searches by location are cached in memory and invalidated when entire collection is modified (a new one uploaded)
* PUT /adplatforms - upload new collection of ad platforms which erase what service already has in memory and invalidates all related caches. Method ignores lines with invalid format and process only valid lines. The result of call is number of processed lines. Valid format is {name}:{locations_list}, where locations list is a set of locations (perhaps duplicated) separated by comma. Each individual location should start with slash symbol and consist of a number of nested locations (at least one) separated by slash, e.g. /ru or /ru/msk/khamovniki

# Задача. Рекламные площадки. (C#)
Рекламодатели часто хотят размещать рекламу в каком-то конкретном регионе
(локации), например только в московской области или только в городе Малые Васюки.
Мы хотим сделать сервис, который помог бы подбирать рекламные площадки для
конкретного региона.  
  
Все рекламные площадки перечислены в текстовом файле вместе с локациями, в
которых они действуют.  

# Пример файла:
Яндекс.Директ:/ru  
Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik  
Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl  
Крутая реклама:/ru/svrd  
Здесь Ревдинский рабочий - это рекламная площадка, a /ru/svrd/revda и
/ru/svrd/pervik — локации, в которых действует Ревдинский рабочий
Локации вложены, если одна содержит другую как префикс, например /ru/svrd/ekb
вложена в /ru/svrd, /ru/svrd вложена в /ru, /ru/svrd/ekb вложена в /ru.  
  
Рекламная площадка действует во всех указанных локациях перечисленных через “,”.
Чем меньше вложенность локации, тем глобальнее действует рекламная площадка.  
  
Пример: для локации /ru/msk подходят Газета уральских москвичей и Яндекс.Директ.
Для локации /ru/svrd подходят Яндекс.Директ и Крутая реклама, для /ru/svrd/revda
подходят Яндекс.Директ, Ревдинский рабочий и Крутая реклама, а для локации /ru
подходит только Яндекс.Директ.

# Что нужно сделать:
Необходимо реализовать простой веб сервис, позволяющий хранить и возвращать
списки рекламных площадок для заданной локации в запросе.

# Информация для реализации:
<ul>
  <li>
    Веб сервис должен содержать 2 метода REST API:
    <ol>
      <li>Метод загрузки рекламных площадок из файла (должен полностью перезаписывать всю хранимую информацию).</li>
      <li>Метод поиска списка рекламных площадок для заданной локации.</li>
    </ol>
  </li>
  <li>Данные должны храниться строго в оперативной памяти (in-memory collection).</li>
  <li>Важно получать результат поиска рекламных площадок как можно быстрее.</li>
  <li>Считаем, что операция загрузки файла вызывается очень редко, а операция поиска рекламных площадок очень часто.</li>
  <li>Программа не должна ломаться от некорректных входных данных.</li>
</ul>
