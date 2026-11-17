# Jäsenrekisteri

.NET MAUI Blazor -sovellus jäsentietojen lisäämiseen, selaamiseen, muokkaamiseen ja poistamiseen MongoDB-tietokannassa. Kaikki lomakkeen kentät ovat pakollisia, ja poisto on vahvistettava erikseen.

Laajempi suomenkielinen kehittäjädokumentaatio löytyy [Docs-kansiosta](Docs/README.md).

## Käyttöönotto

1. Käynnistä MongoDB-palvelin ja varmista, että sovelluksella on oikeus lukea ja kirjoittaa sen tietokantaan.
2. Valitse Visual Studiossa kohdealustan framework ja käynnistä sovellus. Esimerkiksi Windowsissa valitse `net11.0-windows10.0.26100.0` ja **Windows Machine**.
3. Sovellus yrittää käynnistyessään muodostaa yhteyden osoitteeseen `mongodb://localhost:27017`. Android-emulaattorissa oletus on `mongodb://10.0.2.2:27017`, jolla emulaattori tavoittaa kehityskoneen. Fyysisellä laitteella tarvitaan laitteen tavoittama palvelinosoite.
4. Jos osoite tai kirjautumistiedot poikkeavat oletuksesta, syötä MongoDB-yhteysosoite sovellukseen ja valitse **Yhdistä**. Osoite säilyy vain sovelluksen käynnistyksen ajan.

Sovellus käyttää tietokantaa `jasenrekisteri` ja kokoelmaa `jasenet`. MongoDB luo ne ensimmäisen tallennuksen yhteydessä. **Päivitä lista** hakee tiedot uudelleen palvelimelta. Jos kirjoituksen tulosta ei saada varmistettua, peruuta toiminto ja päivitä lista ennen uutta yritystä.

## Tietoturva

Paikallinen oletusosoite on tarkoitettu kehityskäyttöön. Jäsentiedot ovat henkilötietoja: tuotantokäytössä suojaa MongoDB verkkorajoituksilla, käyttäjätunnistuksella, vähimmillä tarvittavilla käyttöoikeuksilla ja TLS-salauksella sekä huolehdi varmuuskopioinnista. Sovellus yhdistää tietokantaan suoraan, joten sen käyttäjä saa yhteysosoitteen tunnuksilla myönnetyt oikeudet; älä jaa laajoja tietokantatunnuksia tai tallenna niitä lähdekoodiin.