# Jäsenrekisterin kehittäjädokumentaatio

Tämä dokumentaatio esittelee sovelluksen rakenteen ja toiminnan henkilölle, joka tuntee jo C#:n perusteita. Dokumentaatio kuvaa projektin nykyistä toteutusta; se ei edellytä aiempaa kokemusta MAUIsta, Blazorista tai MongoDB:stä.

## Lukujärjestys

1. [Arkkitehtuuri ja tiedon kulku](arkkitehtuuri.md): mistä osista sovellus muodostuu ja miten ne toimivat yhdessä.
2. [Luokat ja komponentit](luokat-ja-komponentit.md): projektin tärkeimmät tiedostot, luokat, metodit ja käyttöliittymän tilat.
3. [MongoDB ja tietokantayhteys](mongodb.md): yhteyden muodostus, tietorakenne, tietokantaoperaatiot ja virhetilanteet.
4. [Käyttö ja jatkokehitys](kaytto-ja-jatkokehitys.md): käynnistäminen, toiminnan kokeileminen, vianhaku ja uuden kentän lisääminen.

Projektin juuren [README](../README.md) sisältää lyhyet käyttöönotto-ohjeet. Tiedostopolut tässä dokumentaatiossa ovat suhteessa projektin juurikansioon, ellei muuta mainita.

## Sanastoa

| Käsite | Merkitys tässä projektissa |
| --- | --- |
| .NET MAUI | Sovelluskehys, jonka avulla samaa sovellusta voidaan rakentaa eri käyttöjärjestelmille. |
| Blazor | Tekniikka, jolla käyttöliittymää rakennetaan Razor-komponenteista käyttäen HTML:ää ja C#:ia. |
| Razor-komponentti | `.razor`-tiedosto, jossa voi olla HTML-merkintää, Blazorin ohjeita ja C#-koodia. |
| WebView | Sovelluksen sisällä oleva näkymä, joka näyttää verkkotekniikoilla tehdyn käyttöliittymän. |
| MongoDB | Dokumenttitietokanta, joka tallentaa tietoja kokoelmiin dokumentteina. |
| Dokumentti | Yhden jäsenen tiedot MongoDB:ssä. Dokumentti muistuttaa JSON-oliota, mutta tietokannassa käytetään BSON-muotoa. |
| Kokoelma | Ryhmä dokumentteja; tässä jäsenet ovat `jasenet`-kokoelmassa. |
| CRUD | Create, Read, Update, Delete: lisäys, luku, muokkaus ja poisto. |
| Validointi | Syötteen tarkistaminen ennen sen hyväksymistä, esimerkiksi pakollisten kenttien tarkistus. |
| Riippuvuusinjektio | Tapa antaa luokalle tai komponentille sen tarvitsemia palveluita sen sijaan, että se luo ne itse. |