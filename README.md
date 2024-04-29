1. Sprawdzamy, czy produkt o podanym identyfikatorze istnieje. Następniesprawdzamy, czy magazyn o podanym identyfikatorze istnieje. Wartośćilości przekazana w żądaniu powinna być większa niż 0.
2. 2. Możemy dodać produkt do magazynu tylko wtedy, gdy istniejezamówienie zakupu produktu w tabeli Order. Dlatego sprawdzamy, czy wtabeli Order istnieje rekord z IdProduktu i Ilością (Amount), któreodpowiadają naszemu żądaniu. Data utworzenia zamówienia powinnabyć wcześniejsza niż data utworzenia w żądaniu.
3. Sprawdzamy, czy to zamówienie zostało przypadkiem zrealizowane.Sprawdzamy, czy nie ma wiersza z danym IdOrder w tabeliProduct_Warehouse.
4. Aktualizujemy kolumnę FullfilledAt zamówienia na aktualną datę igodzinę. (UPDATE)
5. Wstawiamy rekord do tabeli Product_Warehouse. Kolumna Pricepowinna odpowiadać cenie produktu pomnożonej przez kolumnę Amountz naszego zamówienia. Ponadto wstawiamy wartość CreatedAt zgodniez aktualnym czasem. (INSERT)
6. W wyniku operacji zwracamy wartość klucza głównego wygenerowanegodla rekordu wstawionego do tabeli Product_Warehouse.

