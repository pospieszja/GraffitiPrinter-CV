using Dapper;
using GraffitiPrinter.UI.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

namespace GraffitiPrinter.UI.DAL
{
    public class GraffitiRepository
    {
        private string _connectionString = "User ID=user;Host=8.8.8.8;Port=5432;Encoding=windows-1250;Client Encoding=latin2;Application Name=GraffitiPrinter;Database=database;Pooling=true;";

        internal IDbConnection Connection
        {
            get
            {
                return new NpgsqlConnection(_connectionString);
            }
        }

        public IEnumerable<TransportPackage> GetTransportPackage(int id)
        {
            var sql = @"SELECT
	                        TRIM(i.indeks) AS productid
	                        , (SELECT TRIM(opis) FROM g.mzk_protokoly_poz WHERE id_zrodla1 = p.id_towaru AND promptopisu = 'Składniki produktu') as ingredients
	                        , (SELECT TRIM(opis) FROM g.mzk_protokoly_poz WHERE id_zrodla1 = p.id_towaru AND promptopisu = 'Ograniczenia w stosowaniu') as restrictions
	                        , (SELECT TRIM(opis) FROM g.mzk_protokoly_poz WHERE id_zrodla1 = p.id_towaru AND promptopisu = 'Organic/Bio nr jednostki certyfikującej') as certificateunit
	                        , TRIM(i.nazwa_indeksu) AS productname
	                        , p.ilosc_zamowiona AS quantity
	                        , TRIM(p.jm) AS unit
	                        , TRIM(p.nazwa_opakowania) AS packagetype
	                        , p.ilosc_opakowan AS packagequantity
	                        , TRIM(p.partia_towaru) AS partnumber
	                        , TO_CHAR(g.datasql(p.data_przydatnosci), 'DD-MM-YYYY') AS expirationdate
	                        , TRIM(z.zamowienie_klienta_nr) as referenceordernumber
	                        , TRIM(io.indeks) as referenceitemnumber
	                        , TRIM(k.opis) as origincountry
	                        , ds.kontrahent as suppliercode
                            , TRIM(ROU2(ds.ilosc_w_opakowaniu)||' '||ds.jm) :: text as packageweigth
                            , ki.pole_l1 :: text as externalcompanycode
                            , (SELECT TRIM(opis) FROM g.mzk_protokoly_poz where id_zrodla1 = i.id_indeksu and promptopisu = 'Produkt niebezpieczny') AS isdangerousgood
                        FROM build.mzk_zlecenia_pak_zam_poz p
                        LEFT JOIN g.gm_indeksy i ON (p.id_towaru =  i.id_indeksu)
                        LEFT JOIN build.mzk_zlecenie_pakowania zp ON (zp.id=p.id_pak)
                        LEFT JOIN g.mzk_protokoly_poz pr ON (pr.rodzajzrodla=7 AND pr.id_zrodla1=p.id_towaru AND pr.nr_protokolu=1 AND pr.lp=1)
                        LEFT JOIN g.mzk_zamow_klienta z ON (z.rok=p.rok_zam AND z.nr=p.nr_zam)
                        LEFT JOIN g.gm_indeksy_odbiorcy io ON (io.id_indeksu=p.id_towaru AND io.id_odbiorcy=zp.id_nabywcy)
                        LEFT JOIN g.gm_indeksy_info_dodatkowe d ON (d.lp=45 AND d.id_indeksu=p.id_towaru)
                        LEFT JOIN g.spd_kraje k ON (k.kraj=trim(d.wartosc3))
                        LEFT JOIN g.gm_dostawy ds ON (ds.id=p.id_dostawy)
                        LEFT JOIN g.spd_kontrahenci_info_dodatkow ki ON (ki.id_kontrahenta=zp.id_nabywcy and ki.rodzaj_info=70)
                        WHERE p.id_pak = @Id";

            using (IDbConnection db = Connection)
            {
                db.Open();
                return db.Query<TransportPackage>(sql, new { id = id });
            }
        }

        public IEnumerable<TransportPackageHeader> GetTransportPackageHeaderByPickupDate(DateTime pickupDateFrom, DateTime pickupDateTo, bool onlyOpen, bool onlyClosed)
        {
            string whereClauseSQL = "";

            if (onlyOpen)
            {
                whereClauseSQL = "AND htx_status_awiza_wms <> 40";
            }

            if (onlyClosed)
            {
                whereClauseSQL = "AND htx_status_awiza_wms = 40";
            }

            var subquery = @",(SELECT
                                CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END
                            FROM build.mzk_zlecenia_pak_zamowienia z
                            LEFT JOIN g.gm_dokumenty_pozycje p ON (p.zamow_nr=z.nr_zam_klienta AND p.zamow_rok=z.rok_zam_klienta)
                            LEFT JOIN g.gm_dokumenty_naglowki n ON (n.id=p.id_naglowka)
                            WHERE z.id_pakowania=pak.id AND n.symbol IN ('Wz') AND n.gotowy=1) AS GoodIssueStatus "
                            + @", (SELECT
                                    CASE WHEN COUNT(*) <> 0 THEN 'TAK' ELSE '' END 
                                FROM g.crm_kontrah_namiary cn
                                LEFT JOIN g.crm_kontrah cr ON (cr.id = cn.id_kontrah)
                                WHERE cr.id_kontrahenta = pak.id_nabywcy AND cn.dok_jakosciowe_hortim = 1) AS QualityDocuments ";

            var sql = @"SELECT
	                        pak.id AS transportpackagenumber
                            , pak.status AS graffitistatus
	                        , to_char(datasql(pak.data_podjecia), 'DD.MM.YYYY') AS pickupdate
	                        , trans.nr_listu_przew AS trackingnumber
	                        , TRIM(u.uzytkownik) AS issuer
                            , (SELECT numer_pelny FROM build.mzk_awizo_wms_naglowek WHERE trnid = pak.htx_id_awiza_wms) AS transportpackageanteeonumber
                            , (SELECT TRIM(skrot_nazwy) FROM g.spd_kontrahenci WHERE id_kontrahenta = pak.id_nabywcy) AS customername
                            , to_char(datasql(data_zlecenia) + timesql(czas_zlecenia), 'DD.MM.YYYY HH24:MM:SS') AS createdat
                            , trans.numer_dok_zdn AS collectiveproofnumber
                            , STRING_AGG((SELECT TRIM(opispalety) FROM g.mzk_palety WHERE kodpalety = zz.kodpalety AND zz.kodpalety <> '99'), ' ') AS packageunit
                            , SUM(zz.ilosc) AS packagequantityheader
                            , (SELECT TRIM(opis_uslugi) FROM build.mzk_zlec_transp_typ_uslugi WHERE symbol_uslugi = trans.typ_uslugi LIMIT 1) AS type
                            , pak.uwagi_zlecenie AS info
                            , pak.htx_czy_wydrukowane_logit AS IsPrinted
                            , pak.htx_status_awiza_wms AS status
                            , CASE WHEN pak.faktura_z_towarem = 1 THEN '' ELSE 'TAK' END AS ElectronicInvoice
                            , TRIM(build.polskie_znaki_utf_latin(trans.opis_statusu_zlec_transp)) AS SchenkerStatus"
                            + subquery +
                        @"FROM build.mzk_zlecenie_pakowania AS pak
                        LEFT OUTER JOIN build.mzk_zlecenie_transport AS trans ON (pak.id = trans.nr_zlec_pak)
                        LEFT OUTER JOIN g.adm_uzytkownicy u ON (u.id_uzytkownika=pak.id_os_zlecajacej)
                        LEFT OUTER JOIN build.mzk_zlecenia_pak_zamowienia pakzam ON (pakzam.id_pakowania = pak.id)
                        LEFT OUTER JOIN g.mzk_palety_zamkli zz ON (zz.nr_zamkli=pakzam.nr_zam_klienta AND zz.rok_zamkli=pakzam.rok_zam_klienta)
                        WHERE datasql(data_podjecia) >= @pickupDateFrom AND datasql(data_podjecia) <= @pickupDateTo
                            AND zz.rok_zamkli > 0"
                        + whereClauseSQL +
                        @"GROUP BY transportpackagenumber, graffitistatus, pickupdate, trackingnumber, issuer, transportpackageanteeonumber, pak.id_nabywcy,
						    createdat, collectiveproofnumber, type, info, IsPrinted, pak.htx_status_awiza_wms, ElectronicInvoice, SchenkerStatus
                        ORDER BY pak.id;";

            using (IDbConnection db = Connection)
            {
                db.Open();
                return db.Query<TransportPackageHeader>(sql, new { pickupDateFrom = pickupDateFrom, pickupDateTo = pickupDateTo });
            }
        }

        public IEnumerable<TransportPackageHeader> GetTransportPackageHeaderById(int id)
        {
            var subquery = @",(SELECT
                                CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END
                            FROM build.mzk_zlecenia_pak_zamowienia z
                            LEFT JOIN g.gm_dokumenty_pozycje p ON (p.zamow_nr=z.nr_zam_klienta AND p.zamow_rok=z.rok_zam_klienta)
                            LEFT JOIN g.gm_dokumenty_naglowki n ON (n.id=p.id_naglowka)
                            WHERE z.id_pakowania=pak.id AND n.symbol IN ('Wz') AND n.gotowy=1) AS GoodIssueStatus "
                            + @", (SELECT
                                    CASE WHEN COUNT(*) <> 0 THEN 'TAK' ELSE '' END 
                                FROM g.crm_kontrah_namiary cn
                                LEFT JOIN g.crm_kontrah cr ON (cr.id = cn.id_kontrah)
                                WHERE cr.id_kontrahenta = pak.id_nabywcy AND cn.dok_jakosciowe_hortim = 1) AS QualityDocuments ";

            var sql = @"SELECT
	                        pak.id AS transportpackagenumber
                            , pak.status AS graffitistatus
	                        , to_char(datasql(pak.data_podjecia), 'DD.MM.YYYY') AS pickupdate
	                        , trans.nr_listu_przew AS trackingnumber
	                        , TRIM(u.uzytkownik) AS issuer
                            , (SELECT numer_pelny FROM build.mzk_awizo_wms_naglowek WHERE trnid = pak.htx_id_awiza_wms) AS transportpackageanteeonumber
                            , (SELECT TRIM(skrot_nazwy) FROM g.spd_kontrahenci WHERE id_kontrahenta = pak.id_nabywcy) AS customername
                            , to_char(datasql(data_zlecenia) + timesql(czas_zlecenia), 'DD.MM.YYYY HH24:MM:SS') AS createdat
                            , trans.numer_dok_zdn AS collectiveproofnumber
                            , STRING_AGG((SELECT TRIM(opispalety) FROM g.mzk_palety WHERE kodpalety = zz.kodpalety AND zz.kodpalety <> '99'), ' ') AS packageunit
                            , SUM(zz.ilosc) AS packagequantityheader
                            , (SELECT TRIM(opis_uslugi) FROM build.mzk_zlec_transp_typ_uslugi WHERE symbol_uslugi = trans.typ_uslugi) AS type
                            , pak.uwagi_zlecenie AS info
                            , pak.htx_czy_wydrukowane_logit AS IsPrinted
                            , pak.htx_status_awiza_wms AS status
                            , CASE WHEN pak.faktura_z_towarem = 1 THEN '' ELSE 'TAK' END AS ElectronicInvoice
                            , TRIM(build.polskie_znaki_utf_latin(trans.opis_statusu_zlec_transp)) AS SchenkerStatus"
                            + subquery +
                        @"FROM build.mzk_zlecenie_pakowania AS pak
                        LEFT OUTER JOIN build.mzk_zlecenie_transport AS trans ON (pak.id = trans.nr_zlec_pak)
                        LEFT OUTER JOIN g.adm_uzytkownicy u ON (u.id_uzytkownika=pak.id_os_zlecajacej)
                        LEFT OUTER JOIN build.mzk_zlecenia_pak_zamowienia pakzam ON (pakzam.id_pakowania = pak.id)
                        LEFT OUTER JOIN g.mzk_palety_zamkli zz ON (zz.nr_zamkli=pakzam.nr_zam_klienta AND zz.rok_zamkli=pakzam.rok_zam_klienta)
                        WHERE pak.id = @id AND zz.rok_zamkli > 0
						GROUP BY transportpackagenumber, graffitistatus, pickupdate, trackingnumber, issuer, transportpackageanteeonumber, pak.id_nabywcy,
						    createdat, collectiveproofnumber, type, info, IsPrinted, pak.htx_status_awiza_wms, ElectronicInvoice, SchenkerStatus;";

            using (IDbConnection db = Connection)
            {
                db.Open();
                return db.Query<TransportPackageHeader>(sql, new { id = id });
            }
        }

        public CollectiveProof GetCollectiveProofInformation(int id)
        {
            var sql = @"SELECT id_paczki_zdn AS packageid, id_paczki_zdn_lp AS packageorder, numer_dok_zdn AS collectivenumber FROM build.mzk_zlecenie_transport WHERE nr_zlec_pak = @Id";

            using (IDbConnection db = Connection)
            {
                db.Open();
                return db.QuerySingleOrDefault<CollectiveProof>(sql, new { id = id });
            }
        }

        public IEnumerable<GraffitiDocument> GetGraffitiDocument(int nr_dokumentu, int typ_dokumentu)
        {
            //typ_dokumentu 
            // 1 - dokumenty jakościowe
            // 2 - faktury
            var sql = @"SELECT
                            identyfikatorpliku AS name
                            , obiekt_typ AS type
                            , obiekt AS binary
                        FROM build.htx_pobierz_dokument(@nr_dokumentu, @typ_dokumentu)";

            using (IDbConnection db = Connection)
            {
                db.Open();
                return db.Query<GraffitiDocument>(sql, new { nr_dokumentu, typ_dokumentu });
            }
        }

        public void SetIssuedToCarrier(int transportHeaderId)
        {
            var sql = @"UPDATE build.mzk_zlecenie_pakowania SET status=4 WHERE id = @transportHeaderId;
                        UPDATE g.tra_zlecenia SET status=4 WHERE nr = @transportHeaderId;";

            using (IDbConnection db = Connection)
            {
                db.Open();
                db.Execute(sql, new { transportHeaderId });
            }
        }

        public void SetTransportPackageAsPrinted(int transportHeaderId)
        {
            var sql = @"UPDATE build.mzk_zlecenie_pakowania SET htx_czy_wydrukowane_logit = true WHERE id = @transportHeaderId;";

            using (IDbConnection db = Connection)
            {
                db.Open();
                db.Execute(sql, new { transportHeaderId });
            }
        }

        public void SetUnlockOrder(int transportHeaderId)
        {
            var sql = @"UPDATE build.mzk_zlecenie_pakowania SET status=2 WHERE id = @transportHeaderId;";

            using (IDbConnection db = Connection)
            {
                db.Open();
                db.Execute(sql, new { transportHeaderId });
            }
        }
    }
}
