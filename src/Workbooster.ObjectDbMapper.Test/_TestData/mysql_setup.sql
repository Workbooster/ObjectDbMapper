DROP TABLE IF EXISTS `addresses`;
DROP TABLE IF EXISTS `people`;

CREATE TABLE `addresses` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `IdPerson` int(11) NOT NULL,
  `Street` varchar(50) NOT NULL,
  `ZipCode` varchar(20) NOT NULL,
  `City` varchar(50) NOT NULL,
  `IsPrimary` tinyint(4) NOT NULL,
  `DateOfCreation` datetime NOT NULL DEFAULT '1900-01-01 00:00:00',
  PRIMARY KEY (`Id`),
  KEY `IdPerson` (`IdPerson`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=12 ;

CREATE TABLE `people` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  `IsMarried` tinyint(4) NOT NULL,
  `DateOfBirth` date NOT NULL,
  `PlaceOfBirth` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=11 ;

ALTER TABLE `addresses`
  ADD CONSTRAINT `addresses_ibfk_1` FOREIGN KEY (`IdPerson`) REFERENCES `people` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE;


INSERT INTO `People` (`Id`, `Name`, `IsMarried`, `DateOfBirth`, `PlaceOfBirth`) VALUES (2, 'Mike', 0, '1985-06-13', NULL);
INSERT INTO `People` (`Id`, `Name`, `IsMarried`, `DateOfBirth`, `PlaceOfBirth`) VALUES (3, 'Steve', 0, '1978-02-03', 'Toronto');
INSERT INTO `People` (`Id`, `Name`, `IsMarried`, `DateOfBirth`, `PlaceOfBirth`) VALUES (4, 'Meg', 1, '1965-03-09', NULL);
INSERT INTO `People` (`Id`, `Name`, `IsMarried`, `DateOfBirth`, `PlaceOfBirth`) VALUES (5, 'Melanie', 0, '1988-11-27', NULL);
INSERT INTO `People` (`Id`, `Name`, `IsMarried`, `DateOfBirth`, `PlaceOfBirth`) VALUES (6, 'Becky', 0, '1972-08-21', NULL);
INSERT INTO `People` (`Id`, `Name`, `IsMarried`, `DateOfBirth`, `PlaceOfBirth`) VALUES (7, 'Larry', 0, '1969-01-26', NULL);
INSERT INTO `People` (`Id`, `Name`, `IsMarried`, `DateOfBirth`, `PlaceOfBirth`) VALUES (8, 'Mike', 1, '1953-09-23', 'Halifax');
INSERT INTO `People` (`Id`, `Name`, `IsMarried`, `DateOfBirth`, `PlaceOfBirth`) VALUES (9, 'mike', 0, '1972-02-22', NULL);
INSERT INTO `People` (`Id`, `Name`, `IsMarried`, `DateOfBirth`, `PlaceOfBirth`) VALUES (10, 'Samuel', 1, '1958-11-27', NULL);


INSERT INTO `Addresses` (`Id`, `IdPerson`, `Street`, `ZipCode`, `City`, `IsPrimary`, `DateOfCreation`) VALUES (1, 2, 'Schaffhauserstr. 119', '8005', 'Zurich', 1, '2014-01-15 00:00:00');
INSERT INTO `Addresses` (`Id`, `IdPerson`, `Street`, `ZipCode`, `City`, `IsPrimary`, `DateOfCreation`) VALUES (2, 2, 'Kempttalstr. 70', '8640', 'Fehraltorf', 0, '2014-03-27 00:00:00');
INSERT INTO `Addresses` (`Id`, `IdPerson`, `Street`, `ZipCode`, `City`, `IsPrimary`, `DateOfCreation`) VALUES (3, 3, 'Baslerstrasse. 320', '8004', 'Zurich', 1, '2014-05-22 00:00:00');
INSERT INTO `Addresses` (`Id`, `IdPerson`, `Street`, `ZipCode`, `City`, `IsPrimary`, `DateOfCreation`) VALUES (4, 4, 'Wehntalerstr. 89', '8157', 'Dielsdorf', 1, '2014-03-11 00:00:00');
INSERT INTO `Addresses` (`Id`, `IdPerson`, `Street`, `ZipCode`, `City`, `IsPrimary`, `DateOfCreation`) VALUES (5, 5, 'Grafschaftstr. 52', '8172', 'Niederglatt', 0, '2014-11-22 00:00:00');
INSERT INTO `Addresses` (`Id`, `IdPerson`, `Street`, `ZipCode`, `City`, `IsPrimary`, `DateOfCreation`) VALUES (6, 5, 'Grafschaftstr. 54', '8172', 'Niederglatt', 1, '2014-06-21 00:00:00');
INSERT INTO `Addresses` (`Id`, `IdPerson`, `Street`, `ZipCode`, `City`, `IsPrimary`, `DateOfCreation`) VALUES (7, 5, 'Via A. Ciseri 13', '6600', 'Locarno', 0, '2014-08-01 00:00:00');
INSERT INTO `Addresses` (`Id`, `IdPerson`, `Street`, `ZipCode`, `City`, `IsPrimary`, `DateOfCreation`) VALUES (8, 6, 'Seetalstrasse 6', '5630', 'Muri', 1, '2014-07-09 00:00:00');
INSERT INTO `Addresses` (`Id`, `IdPerson`, `Street`, `ZipCode`, `City`, `IsPrimary`, `DateOfCreation`) VALUES (9, 7, 'Albisriederstr 167', '8047', 'Zurich', 0, '2014-12-17 00:00:00');
INSERT INTO `Addresses` (`Id`, `IdPerson`, `Street`, `ZipCode`, `City`, `IsPrimary`, `DateOfCreation`) VALUES (10, 7, 'Jurastrasse 22', '4901', 'Langenthal', 1, '2014-04-01 00:00:00');
INSERT INTO `Addresses` (`Id`, `IdPerson`, `Street`, `ZipCode`, `City`, `IsPrimary`, `DateOfCreation`) VALUES (11, 7, 'Casa Calanca', '6633', 'Lavertezzo', 0, '2014-09-15 00:00:00');
