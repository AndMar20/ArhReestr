-- MySQL dump 10.13  Distrib 8.0.38, for Win64 (x86_64)
--
-- Host: mysql    Database: ispp2108
-- ------------------------------------------------------
-- Server version	8.0.39

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `Districts`
--
use ispp2108;
DROP TABLE IF EXISTS `Districts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Districts` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_district_name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Districts`
--

LOCK TABLES `Districts` WRITE;
/*!40000 ALTER TABLE `Districts` DISABLE KEYS */;
INSERT INTO `Districts` VALUES
(1,'Ломоносовский'),
(2,'Октябрьский'),
(3,'Соломбальский'),
(4,'Майская Горка'),
(6,'Варавино-Фактория');
/*!40000 ALTER TABLE `Districts` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `InteractionStatuses`
--

DROP TABLE IF EXISTS `InteractionStatuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `InteractionStatuses` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `InteractionStatuses`
--

LOCK TABLES `InteractionStatuses` WRITE;
/*!40000 ALTER TABLE `InteractionStatuses` DISABLE KEYS */;
INSERT INTO `InteractionStatuses` VALUES
(1,'Контакт установлен'),
(2,'Назначена встреча'),
(3,'Отказ'),
(4,'Сделка в работе'),
(5,'Сделка завершена');
/*!40000 ALTER TABLE `InteractionStatuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `RealEstateTypes`
--

DROP TABLE IF EXISTS `RealEstateTypes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `RealEstateTypes` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `RealEstateTypes`
--

LOCK TABLES `RealEstateTypes` WRITE;
/*!40000 ALTER TABLE `RealEstateTypes` DISABLE KEYS */;
INSERT INTO `RealEstateTypes` VALUES
(1,'Квартира'),
(2,'Дом'),
(3,'Таунхаус');
/*!40000 ALTER TABLE `RealEstateTypes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `RealEstateStatuses`
--

DROP TABLE IF EXISTS `RealEstateStatuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `RealEstateStatuses` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `code` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_real_estate_status_code` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

LOCK TABLES `RealEstateStatuses` WRITE;
/*!40000 ALTER TABLE `RealEstateStatuses` DISABLE KEYS */;
INSERT INTO `RealEstateStatuses` VALUES
(1,'Черновик','draft'),
(2,'Активен','active'),
(3,'Забронирован','reserved'),
(4,'Продан','sold'),
(5,'Архив','archive');
/*!40000 ALTER TABLE `RealEstateStatuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Roles`
--

DROP TABLE IF EXISTS `Roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Roles` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `displayName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Roles`
--

LOCK TABLES `Roles` WRITE;
/*!40000 ALTER TABLE `Roles` DISABLE KEYS */;
INSERT INTO `Roles` VALUES
(1,'admin','Администратор'),
(2,'agent','Риелтор'),
(3,'client','Клиент');
/*!40000 ALTER TABLE `Roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Streets`
--

DROP TABLE IF EXISTS `Streets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Streets` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_street_name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Streets`
--

LOCK TABLES `Streets` WRITE;
/*!40000 ALTER TABLE `Streets` DISABLE KEYS */;
INSERT INTO `Streets` VALUES
(9, 'пр. Ленинградский'),
(21, 'пр. Троицкий'),
(10, 'ул. Адмирала Кузнецова'),
(5, 'ул. Воронина'),
(2, 'ул. Воскресенская'),
(4, 'ул. Гайдара'),
(7, 'ул. Карпогорская'),
(19, 'ул. Набережная Северной Двины'),
(6, 'ул. Никитова'),
(16, 'ул. Первомайская'),
(18, 'ул. Поморская'),
(14, 'ул. Попова'),
(20, 'ул. Садовая'),
(15, 'ул. Силикатчиков'),
(17, 'ул. Смольная'),
(13, 'ул. Советская'),
(8, 'ул. Тимме'),
(11, 'ул. Урицкого'),
(12, 'ул. Чумбарова-Лучинского');
/*!40000 ALTER TABLE `Streets` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Houses`
--

DROP TABLE IF EXISTS `Houses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Houses` (
  `id` int NOT NULL AUTO_INCREMENT,
  `streetId` int NOT NULL,
  `districtId` int NOT NULL,
  `number` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `totalFloors` int NOT NULL,
  `hasParking` tinyint(1) DEFAULT '0',
  `hasElevator` tinyint(1) DEFAULT '0',
  `buildingYear` int DEFAULT NULL,
  `latitude` decimal(10,7) DEFAULT NULL,
  `longitude` decimal(10,7) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_house_address` (`districtId`,`streetId`,`number`),
  KEY `idx_street` (`streetId`),
  KEY `idx_district` (`districtId`),
  KEY `idx_house_street_number` (`streetId`,`number`),
  CONSTRAINT `Houses_fk_street` FOREIGN KEY (`streetId`) REFERENCES `Streets` (`id`) ON DELETE RESTRICT,
  CONSTRAINT `Houses_fk_district` FOREIGN KEY (`districtId`) REFERENCES `Districts` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Houses`
--

LOCK TABLES `Houses` WRITE;
/*!40000 ALTER TABLE `Houses` DISABLE KEYS */;
INSERT INTO `Houses` VALUES
(3, 2, 1, '15', 5, 0, 0, 1968, 64.5436000, 40.5205000),
(5, 4, 4, '24', 10, 1, 1, 2008, 64.5195000, 40.5938000),
(6, 5, 6, '28', 2, 1, 0, 2010, 64.4995000, 40.6735000),
(7, 6, 6, '9', 2, 1, 0, 2008, 64.4969000, 40.6814000),
(8, 7, 4, '14', 3, 1, 0, 2018, 64.5209000, 40.6132000),
(9, 8, 2, '24', 9, 1, 1, 1995, 64.5485000, 40.5554000),
(10, 9, 4, '12', 4, 1, 0, 2015, 64.5222000, 40.6011000),
(11, 10, 3, '15', 9, 1, 1, 1982, 64.5821000, 40.5329000),
(12, 11, 1, '8', 5, 0, 0, 1970, 64.5398000, 40.5227000),
(13, 12, 1, '10', 4, 0, 0, 1955, 64.5411733, 40.5206520),
(14, 13, 1, '22', 5, 0, 0, 1960, 64.5402000, 40.5287000),
(15, 14, 1, '5', 9, 1, 1, 1988, 64.5379000, 40.5235000),
(17, 16, 2, '30', 9, 1, 1, 1985, 64.5532000, 40.5607000),
(18, 17, 2, '7', 9, 0, 1, 1973, 64.5528000, 40.5486000),
(19, 18, 1, '16', 5, 0, 0, 1965, 64.5369000, 40.5146000),
(20, 19, 1, '90', 9, 1, 1, 2000, 64.5392000, 40.5109000),
(21, 20, 1, '3', 2, 0, 0, 1962, 64.5386000, 40.5279000),
(22, 21, 1, '20', 9, 1, 1, 1975, 64.5407000, 40.5179000),
(23, 21, 2, '61', 9, 1, 1, 1985, 64.5419000, 40.5356000),
(24, 19, 1, '71', 9, 1, 1, 2005, 64.5412000, 40.5095000),
(25, 15, 6, '8', 2, 1, 0, 1977, 64.4593000, 40.7194000);
/*!40000 ALTER TABLE `Houses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Users`
--

DROP TABLE IF EXISTS `Users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Users` (
  `id` int NOT NULL AUTO_INCREMENT,
  `lastName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `firstName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `middleName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `phone` varchar(15) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `passwordHash` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `roleId` int NOT NULL,
  `createdAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `deletedAt` datetime DEFAULT NULL,
  `phoneVerified` tinyint(1) DEFAULT '0',
  `emailVerified` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `email` (`email`),
  KEY `roleId` (`roleId`),
  CONSTRAINT `Users_ibfk_1` FOREIGN KEY (`roleId`) REFERENCES `Roles` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Users`
--

LOCK TABLES `Users` WRITE;
/*!40000 ALTER TABLE `Users` DISABLE KEYS */;
INSERT INTO `Users` VALUES
(1,'Иванов','Иван','Иванович','+79111111111','admin@mail.ru','AQAAAAIAAYagAAAAEBwhIPUqO+ccfI/7qDL9j9oxM/9TsJnCZV2MR/GicXNoI1MzPBVpabZDizh/9Kyl/g==',1,'2025-11-01 10:06:31',NULL,1,1),
(2,'Петрова','Анна','Сергеевна','+79112223344','petrova@mail.ru','AQAAAAIAAYagAAAAEEekakrE/fX41GJU58+uqgkn2CpvOxYLxcapPWMHyvuxsS+C5796g7BbxDcmrMJJkg==',2,'2025-11-01 10:06:31',NULL,1,1),
(3,'Сидоров','Дмитрий','Викторович','+79115556677','sidorov@mail.ru','AQAAAAIAAYagAAAAEEekakrE/fX41GJU58+uqgkn2CpvOxYLxcapPWMHyvuxsS+C5796g7BbxDcmrMJJkg==',2,'2025-11-01 10:06:31',NULL,1,1),
(4,'Кузнецов','Алексей',NULL,'+79118889900','kuznetsov@mail.ru','AQAAAAIAAYagAAAAEJlZBZ4cN+oLvwWgrjZHaDw0jGv3eRvN3taxUFPXgwWhbymBF4XwTLKsb4VKIGLNMw==',3,'2025-11-01 10:06:31',NULL,0,1),
(5,'Морозова','Елена',NULL,'+79117776655','morozova@mail.ru','AQAAAAIAAYagAAAAEJlZBZ4cN+oLvwWgrjZHaDw0jGv3eRvN3taxUFPXgwWhbymBF4XwTLKsb4VKIGLNMw==',3,'2025-11-01 10:06:31',NULL,0,1),
(6,'Григорьев','Олег',NULL,'+79114443322','grigoryev@mail.ru','AQAAAAIAAYagAAAAEJlZBZ4cN+oLvwWgrjZHaDw0jGv3eRvN3taxUFPXgwWhbymBF4XwTLKsb4VKIGLNMw==',3,'2025-11-01 10:06:31',NULL,0,1);
/*!40000 ALTER TABLE `Users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `RealEstate`
--

DROP TABLE IF EXISTS `RealEstate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `RealEstate` (
  `id` int NOT NULL AUTO_INCREMENT,
  `agentId` int NOT NULL,
  `typeId` int NOT NULL,
  `houseId` int NOT NULL,
  `statusId` int NOT NULL DEFAULT '2',
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `price` decimal(12,2) NOT NULL,
  `rooms` int NOT NULL,
  `area` decimal(8,2) NOT NULL,
  `floor` int NOT NULL,
  `hasBalcony` tinyint(1) DEFAULT '0',
  `createdAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `deletedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_type` (`typeId`),
  KEY `idx_price_area` (`price`,`area`),
  KEY `idx_real_estate_deleted_created` (`deletedAt`,`createdAt`),
  KEY `idx_real_estate_house_deleted` (`houseId`,`deletedAt`),
  KEY `idx_real_estate_status_deleted` (`statusId`,`deletedAt`),
  KEY `idx_agent` (`agentId`),
  KEY `idx_house` (`houseId`),
  CONSTRAINT `RealEstate_fk_agent` FOREIGN KEY (`agentId`) REFERENCES `Users` (`id`) ON DELETE CASCADE,
  CONSTRAINT `RealEstate_fk_type` FOREIGN KEY (`typeId`) REFERENCES `RealEstateTypes` (`id`) ON DELETE RESTRICT,
  CONSTRAINT `RealEstate_fk_house` FOREIGN KEY (`houseId`) REFERENCES `Houses` (`id`) ON DELETE RESTRICT,
  CONSTRAINT `RealEstate_fk_status` FOREIGN KEY (`statusId`) REFERENCES `RealEstateStatuses` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `RealEstate`
-- 30 объектов недвижимости по Архангельску
--

LOCK TABLES `RealEstate` WRITE;
/*!40000 ALTER TABLE `RealEstate` DISABLE KEYS */;
INSERT INTO `RealEstate` VALUES
(1, 2, 1, 22, 2, '1-комнатная квартира в центре Архангельска, просп. Троицкий, д. 20, Ломоносовский округ.', 4100000.00, 1, 33.40, 5, 1, '2025-11-01 10:00:00', NULL),
(2, 3, 1, 3, 2, 'Уютная 1-комнатная квартира на ул. Воскресенская, д. 15, рядом площадь Ленина.', 3550000.00, 1, 31.20, 3, 1, '2025-11-02 11:15:00', NULL),
(3, 2, 1, 24, 1, 'Современная студия на наб. Северной Двины, д. 71, вид на реку.', 3900000.00, 1, 29.80, 7, 1, '2025-11-03 14:20:00', NULL),
(4, 3, 1, 12, 2, '1-комнатная квартира в историческом центре, ул. Урицкого, д. 8, рядом набережная.', 4200000.00, 1, 34.50, 2, 1, '2025-11-04 09:45:00', NULL),
(5, 2, 1, 13, 2, 'Квартира-студия на пешеходной улице Чумбарова-Лучинского, д. 10.', 3800000.00, 1, 28.90, 4, 0, '2025-11-05 16:10:00', NULL),
(6, 3, 1, 14, 3, '2-комнатная квартира на ул. Советская, д. 22, Ломоносовский округ.', 5800000.00, 2, 52.30, 3, 1, '2025-11-06 12:30:00', NULL),
(7, 2, 1, 15, 2, '2-комнатная квартира с раздельными комнатами, ул. Попова, д. 5.', 5650000.00, 2, 50.10, 6, 1, '2025-11-07 18:05:00', NULL),
(8, 3, 1, 19, 4, '2-комнатная квартира на ул. Поморская, д. 16, рядом набережная.', 6100000.00, 2, 55.80, 4, 1, '2025-11-08 13:40:00', NULL),
(9, 2, 1, 20, 2, '2-комнатная квартира на ул. Набережная Северной Двины, д. 90, вид на реку.', 6400000.00, 2, 57.40, 8, 1, '2025-11-09 10:55:00', NULL),
(10, 3, 1, 21, 2, 'Семейная 2-комнатная квартира на ул. Садовая, д. 3, тихий центр.', 5200000.00, 2, 49.50, 2, 0, '2025-11-10 17:25:00', NULL),
(11, 2, 1, 23, 3, '3-комнатная квартира на просп. Троицкий, д. 61, Октябрьский округ.', 8900000.00, 3, 82.40, 7, 1, '2025-11-11 11:00:00', NULL),
(12, 3, 1, 9, 2, 'Современная 2-комнатная квартира на ул. Тимме, д. 24, рядом школы и детсады.', 6500000.00, 2, 60.00, 5, 1, '2025-11-12 15:35:00', NULL),
(13, 2, 1, 17, 2, '2-комнатная квартира на ул. Первомайская, д. 30, Октябрьский округ.', 5700000.00, 2, 53.20, 4, 1, '2025-11-13 09:20:00', NULL),
(14, 3, 1, 18, 5, '1-комнатная квартира в спальном районе, ул. Смольная, д. 7.', 3300000.00, 1, 32.00, 5, 1, '2025-11-14 19:10:00', NULL),
(15, 2, 1, 8, 2, '2-комнатная квартира в новом доме на Карпогорской, д. 14, район Майская Горка.', 7200000.00, 2, 59.40, 9, 1, '2025-11-15 10:50:00', NULL),
(16, 3, 1, 5, 2, '1-комнатная квартира на ул. Гайдара, д. 24, микрорайон Майская Горка.', 4500000.00, 1, 35.10, 8, 1, '2025-11-16 12:05:00', NULL),
(17, 2, 1, 10, 2, '3-комнатная квартира на пр. Ленинградский, д. 12, рядом ТРЦ и остановка.', 9700000.00, 3, 86.30, 10, 1, '2025-11-17 14:45:00', NULL),
(18, 3, 1, 11, 2, '2-комнатная квартира в Соломбале, ул. Адмирала Кузнецова, д. 15.', 4800000.00, 2, 48.20, 4, 1, '2025-11-18 18:30:00', NULL),
(19, 2, 1, 11, 2, '3-комнатная квартира в Соломбале, ул. Адмирала Кузнецова, д. 15, вид на Северную Двину.', 6900000.00, 3, 74.10, 7, 1, '2025-11-19 09:15:00', NULL),
(20, 3, 1, 24, 2, '3-комнатная квартира на наб. Северной Двины, д. 71, рядом центр города.', 8300000.00, 3, 80.50, 8, 1, '2025-11-20 16:20:00', NULL),
(21, 2, 2, 6, 2, 'Частный дом в районе Варавино-Фактория, ул. Воронина, д. 28, участок 8 соток.', 11200000.00, 4, 130.00, 1, 0, '2025-11-21 11:25:00', NULL),
(22, 3, 2, 7, 2, 'Дом в районе Никитова, д. 9, Варавино-Фактория, все коммуникации.', 9500000.00, 4, 120.00, 1, 0, '2025-11-22 13:15:00', NULL),
(23, 2, 2, 25, 2, 'Дом в микрорайоне Белая Гора, ул. Силикатчиков, д. 8, рядом река.', 8700000.00, 3, 110.00, 1, 0, '2025-11-23 17:40:00', NULL),
(24, 3, 2, 7, 1, 'Небольшой дом в Варавино, ул. Никитова, д. 9, подходит для круглогодичного проживания.', 7800000.00, 3, 95.00, 1, 0, '2025-11-24 10:35:00', NULL),
(25, 2, 3, 8, 3, 'Таунхаус в современном комплексе на Карпогорской, д. 14, три уровня.', 13500000.00, 4, 160.00, 1, 1, '2025-11-25 15:55:00', NULL),
(26, 3, 3, 5, 2, 'Таунхаус в районе Майская Горка, ул. Гайдара, д. 24, отдельный вход и парковка.', 12800000.00, 4, 150.00, 1, 1, '2025-11-26 12:45:00', NULL),
(27, 2, 3, 10, 2, 'Таунхаус у пр. Ленинградский, д. 12, закрытый двор.', 14200000.00, 5, 175.00, 1, 1, '2025-11-27 09:50:00', NULL),
(28, 3, 2, 22, 2, 'Большой дом в центральной части города, просп. Троицкий, д. 20, отдельный вход с двора.', 20000000.00, 6, 210.00, 1, 0, '2025-11-28 18:15:00', NULL),
(29, 2, 2, 21, 2, 'Дом в старом центре, ул. Садовая, д. 3, ухоженный участок.', 11500000.00, 4, 140.00, 1, 0, '2025-11-29 14:05:00', NULL),
(30, 3, 2, 6, 2, 'Дом в Варавино-Фактории, ул. Воронина, д. 28, два гаража.', 12300000.00, 5, 165.00, 1, 0, '2025-11-30 16:40:00', NULL);
/*!40000 ALTER TABLE `RealEstate` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `RealEstatePhotos`
--

DROP TABLE IF EXISTS `RealEstatePhotos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `RealEstatePhotos` (
  `id` int NOT NULL AUTO_INCREMENT,
  `realEstateId` int NOT NULL,
  `filePath` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `fileName` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `isPrimary` tinyint(1) DEFAULT '0',
  `deletedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_real_estate` (`realEstateId`),
  KEY `idx_real_estate_primary_not_deleted` (`realEstateId`,`isPrimary`,`deletedAt`),
  CONSTRAINT `RealEstatePhotos_fk_real_estate` FOREIGN KEY (`realEstateId`) REFERENCES `RealEstate` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=91 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `RealEstatePhotos`
-- по 3 фото на каждый объект
--

LOCK TABLES `RealEstatePhotos` WRITE;
/*!40000 ALTER TABLE `RealEstatePhotos` DISABLE KEYS */;
INSERT INTO `RealEstatePhotos` VALUES
(1,1,'/Images/RealEstates/1/1.jpg','Фото 1',1,NULL),
(2,1,'/Images/RealEstates/1/2.jpg','Фото 2',0,NULL),
(3,1,'/Images/RealEstates/1/3.jpg','Фото 3',0,NULL),
(4,2,'/Images/RealEstates/2/1.jpg','Фото 1',1,NULL),
(5,2,'/Images/RealEstates/2/2.jpg','Фото 2',0,NULL),
(6,2,'/Images/RealEstates/2/3.jpg','Фото 3',0,NULL),
(7,3,'/Images/RealEstates/3/1.jpg','Фото 1',1,NULL),
(8,3,'/Images/RealEstates/3/2.jpg','Фото 2',0,NULL),
(9,3,'/Images/RealEstates/3/3.jpg','Фото 3',0,NULL),
(10,4,'/Images/RealEstates/4/1.jpg','Фото 1',1,NULL),
(11,4,'/Images/RealEstates/4/2.jpg','Фото 2',0,NULL),
(12,4,'/Images/RealEstates/4/3.jpg','Фото 3',0,NULL),
(13,5,'/Images/RealEstates/5/1.jpg','Фото 1',1,NULL),
(14,5,'/Images/RealEstates/5/2.jpg','Фото 2',0,NULL),
(15,5,'/Images/RealEstates/5/3.jpg','Фото 3',0,NULL),
(16,6,'/Images/RealEstates/6/1.jpg','Фото 1',1,NULL),
(17,6,'/Images/RealEstates/6/2.jpg','Фото 2',0,NULL),
(18,6,'/Images/RealEstates/6/3.jpg','Фото 3',0,NULL),
(19,7,'/Images/RealEstates/7/1.jpg','Фото 1',1,NULL),
(20,7,'/Images/RealEstates/7/2.jpg','Фото 2',0,NULL),
(21,7,'/Images/RealEstates/7/3.jpg','Фото 3',0,NULL),
(22,8,'/Images/RealEstates/8/1.jpg','Фото 1',1,NULL),
(23,8,'/Images/RealEstates/8/2.jpg','Фото 2',0,NULL),
(24,8,'/Images/RealEstates/8/3.jpg','Фото 3',0,NULL),
(25,9,'/Images/RealEstates/9/1.jpg','Фото 1',1,NULL),
(26,9,'/Images/RealEstates/9/2.jpg','Фото 2',0,NULL),
(27,9,'/Images/RealEstates/9/3.jpg','Фото 3',0,NULL),
(28,10,'/Images/RealEstates/10/1.jpg','Фото 1',1,NULL),
(29,10,'/Images/RealEstates/10/2.jpg','Фото 2',0,NULL),
(30,10,'/Images/RealEstates/10/3.jpg','Фото 3',0,NULL),
(31,11,'/Images/RealEstates/11/1.jpg','Фото 1',1,NULL),
(32,11,'/Images/RealEstates/11/2.jpg','Фото 2',0,NULL),
(33,11,'/Images/RealEstates/11/3.jpg','Фото 3',0,NULL),
(34,12,'/Images/RealEstates/12/1.jpg','Фото 1',1,NULL),
(35,12,'/Images/RealEstates/12/2.jpg','Фото 2',0,NULL),
(36,12,'/Images/RealEstates/12/3.jpg','Фото 3',0,NULL),
(37,13,'/Images/RealEstates/13/1.jpg','Фото 1',1,NULL),
(38,13,'/Images/RealEstates/13/2.jpg','Фото 2',0,NULL),
(39,13,'/Images/RealEstates/13/3.jpg','Фото 3',0,NULL),
(40,14,'/Images/RealEstates/14/1.jpg','Фото 1',1,NULL),
(41,14,'/Images/RealEstates/14/2.jpg','Фото 2',0,NULL),
(42,14,'/Images/RealEstates/14/3.jpg','Фото 3',0,NULL),
(43,15,'/Images/RealEstates/15/1.jpg','Фото 1',1,NULL),
(44,15,'/Images/RealEstates/15/2.jpg','Фото 2',0,NULL),
(45,15,'/Images/RealEstates/15/3.jpg','Фото 3',0,NULL),
(46,16,'/Images/RealEstates/16/1.jpg','Фото 1',1,NULL),
(47,16,'/Images/RealEstates/16/2.jpg','Фото 2',0,NULL),
(48,16,'/Images/RealEstates/16/3.jpg','Фото 3',0,NULL),
(49,17,'/Images/RealEstates/17/1.jpg','Фото 1',1,NULL),
(50,17,'/Images/RealEstates/17/2.jpg','Фото 2',0,NULL),
(51,17,'/Images/RealEstates/17/3.jpg','Фото 3',0,NULL),
(52,18,'/Images/RealEstates/18/1.jpg','Фото 1',1,NULL),
(53,18,'/Images/RealEstates/18/2.jpg','Фото 2',0,NULL),
(54,18,'/Images/RealEstates/18/3.jpg','Фото 3',0,NULL),
(55,19,'/Images/RealEstates/19/1.jpg','Фото 1',1,NULL),
(56,19,'/Images/RealEstates/19/2.jpg','Фото 2',0,NULL),
(57,19,'/Images/RealEstates/19/3.jpg','Фото 3',0,NULL),
(58,20,'/Images/RealEstates/20/1.jpg','Фото 1',1,NULL),
(59,20,'/Images/RealEstates/20/2.jpg','Фото 2',0,NULL),
(60,20,'/Images/RealEstates/20/3.jpg','Фото 3',0,NULL),
(61,21,'/Images/RealEstates/21/1.jpg','Фото 1',1,NULL),
(62,21,'/Images/RealEstates/21/2.jpg','Фото 2',0,NULL),
(63,21,'/Images/RealEstates/21/3.jpg','Фото 3',0,NULL),
(64,22,'/Images/RealEstates/22/1.jpg','Фото 1',1,NULL),
(65,22,'/Images/RealEstates/22/2.jpg','Фото 2',0,NULL),
(66,22,'/Images/RealEstates/22/3.jpg','Фото 3',0,NULL),
(67,23,'/Images/RealEstates/23/1.jpg','Фото 1',1,NULL),
(68,23,'/Images/RealEstates/23/2.jpg','Фото 2',0,NULL),
(69,23,'/Images/RealEstates/23/3.jpg','Фото 3',0,NULL),
(70,24,'/Images/RealEstates/24/1.jpg','Фото 1',1,NULL),
(71,24,'/Images/RealEstates/24/2.jpg','Фото 2',0,NULL),
(72,24,'/Images/RealEstates/24/3.jpg','Фото 3',0,NULL),
(73,25,'/Images/RealEstates/25/1.jpg','Фото 1',1,NULL),
(74,25,'/Images/RealEstates/25/2.jpg','Фото 2',0,NULL),
(75,25,'/Images/RealEstates/25/3.jpg','Фото 3',0,NULL),
(76,26,'/Images/RealEstates/26/1.jpg','Фото 1',1,NULL),
(77,26,'/Images/RealEstates/26/2.jpg','Фото 2',0,NULL),
(78,26,'/Images/RealEstates/26/3.jpg','Фото 3',0,NULL),
(79,27,'/Images/RealEstates/27/1.jpg','Фото 1',1,NULL),
(80,27,'/Images/RealEstates/27/2.jpg','Фото 2',0,NULL),
(81,27,'/Images/RealEstates/27/3.jpg','Фото 3',0,NULL),
(82,28,'/Images/RealEstates/28/1.jpg','Фото 1',1,NULL),
(83,28,'/Images/RealEstates/28/2.jpg','Фото 2',0,NULL),
(84,28,'/Images/RealEstates/28/3.jpg','Фото 3',0,NULL),
(85,29,'/Images/RealEstates/29/1.jpg','Фото 1',1,NULL),
(86,29,'/Images/RealEstates/29/2.jpg','Фото 2',0,NULL),
(87,29,'/Images/RealEstates/29/3.jpg','Фото 3',0,NULL),
(88,30,'/Images/RealEstates/30/1.jpg','Фото 1',1,NULL),
(89,30,'/Images/RealEstates/30/2.jpg','Фото 2',0,NULL),
(90,30,'/Images/RealEstates/30/3.jpg','Фото 3',0,NULL);
/*!40000 ALTER TABLE `RealEstatePhotos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Interactions`
--

DROP TABLE IF EXISTS `Interactions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Interactions` (
  `id` int NOT NULL AUTO_INCREMENT,
  `clientId` int NOT NULL,
  `agentId` int NOT NULL,
  `realEstateId` int NOT NULL,
  `statusId` int NOT NULL,
  `contactedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `updatedAt` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `notes` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `deletedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_client_real_estate` (`clientId`,`realEstateId`),
  KEY `idx_client` (`clientId`),
  KEY `idx_agent` (`agentId`),
  KEY `idx_real_estate` (`realEstateId`),
  KEY `idx_status` (`statusId`),
  KEY `idx_contacted` (`contactedAt`),
  KEY `idx_interactions_updated` (`updatedAt`),
  KEY `idx_interactions_agent_status_updated` (`agentId`,`statusId`,`updatedAt`),
  KEY `idx_interactions_status_time` (`statusId`,`contactedAt`),
  CONSTRAINT `Interactions_fk_agent` FOREIGN KEY (`agentId`) REFERENCES `Users` (`id`) ON DELETE CASCADE,
  CONSTRAINT `Interactions_fk_client` FOREIGN KEY (`clientId`) REFERENCES `Users` (`id`) ON DELETE CASCADE,
  CONSTRAINT `Interactions_fk_real_estate` FOREIGN KEY (`realEstateId`) REFERENCES `RealEstate` (`id`) ON DELETE CASCADE,
  CONSTRAINT `Interactions_fk_status` FOREIGN KEY (`statusId`) REFERENCES `InteractionStatuses` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Interactions`
--

LOCK TABLES `Interactions` WRITE;
/*!40000 ALTER TABLE `Interactions` DISABLE KEYS */;
INSERT INTO `Interactions` VALUES
(1,4,2,1,2,'2025-11-01 10:06:31','2025-12-05 11:44:40','Клиент хочет посмотреть в субботу.',NULL),
(2,5,3,4,4,'2025-11-02 12:15:00','2025-11-02 12:15:00','Подписан предварительный договор, ждём одобрения ипотеки.',NULL),
(3,6,3,2,3,'2025-11-03 09:30:00','2025-11-03 09:30:00','Не подошла планировка, клиент рассматривает другие варианты.',NULL),
(4,4,2,6,1,'2025-11-05 11:00:00','2025-11-05 11:00:00','Первичный звонок по объявлению, отправлено видео квартиры.',NULL),
(5,5,3,8,5,'2025-11-06 18:20:00','2025-11-18 16:30:00','Сделка завершена, объект продан.',NULL),
(6,6,2,11,4,'2025-11-08 15:40:00','2025-11-15 16:10:00','Клиент внёс аванс, готовим договор купли-продажи.',NULL),
(7,4,3,15,1,'2025-11-10 13:05:00','2025-11-10 13:05:00','Клиент запросил расчёт ипотеки.',NULL),
(8,5,2,18,2,'2025-11-12 17:25:00','2025-11-12 17:25:00','Показ согласован, встреча у подъезда.',NULL),
(9,6,3,21,1,'2025-11-20 10:50:00','2025-11-20 10:50:00','Интерес к дому в Варавино-Фактории, отправлены документы.',NULL),
(10,4,2,25,4,'2025-11-28 19:00:00','2025-11-28 19:00:00','Клиент выбрал таунхаус, готовится сделка.',NULL);
/*!40000 ALTER TABLE `Interactions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Deals`
--

DROP TABLE IF EXISTS `Deals`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Deals` (
  `id` int NOT NULL AUTO_INCREMENT,
  `interactionId` int NOT NULL,
  `realEstateId` int NOT NULL,
  `agentId` int NOT NULL,
  `clientId` int NOT NULL,
  `amount` decimal(12,2) NOT NULL,
  `commission` decimal(12,2) NOT NULL,
  `closedAt` datetime NOT NULL,
  `createdAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_deal_interaction` (`interactionId`),
  KEY `idx_deal_agent_closed` (`agentId`,`closedAt`),
  KEY `idx_deal_closed` (`closedAt`),
  KEY `idx_deal_real_estate` (`realEstateId`),
  KEY `idx_deal_client` (`clientId`),
  CONSTRAINT `Deals_fk_interaction` FOREIGN KEY (`interactionId`) REFERENCES `Interactions` (`id`) ON DELETE CASCADE,
  CONSTRAINT `Deals_fk_real_estate` FOREIGN KEY (`realEstateId`) REFERENCES `RealEstate` (`id`) ON DELETE CASCADE,
  CONSTRAINT `Deals_fk_agent` FOREIGN KEY (`agentId`) REFERENCES `Users` (`id`) ON DELETE CASCADE,
  CONSTRAINT `Deals_fk_client` FOREIGN KEY (`clientId`) REFERENCES `Users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

LOCK TABLES `Deals` WRITE;
/*!40000 ALTER TABLE `Deals` DISABLE KEYS */;
INSERT INTO `Deals` VALUES
(1,5,8,3,5,6100000.00,183000.00,'2025-11-18 16:30:00','2025-11-18 16:30:00');
/*!40000 ALTER TABLE `Deals` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `UserFavorites`
--

DROP TABLE IF EXISTS `UserFavorites`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `UserFavorites` (
  `id` int NOT NULL AUTO_INCREMENT,
  `userId` int NOT NULL,
  `realEstateId` int NOT NULL,
  `createdAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_user_favorite` (`userId`,`realEstateId`),
  KEY `idx_favorite_real_estate` (`realEstateId`),
  CONSTRAINT `UserFavorites_fk_user` FOREIGN KEY (`userId`) REFERENCES `Users` (`id`) ON DELETE CASCADE,
  CONSTRAINT `UserFavorites_fk_real_estate` FOREIGN KEY (`realEstateId`) REFERENCES `RealEstate` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `UserFavorites`
--

LOCK TABLES `UserFavorites` WRITE;
/*!40000 ALTER TABLE `UserFavorites` DISABLE KEYS */;
INSERT INTO `UserFavorites` VALUES
(1,4,1,'2025-12-02 10:00:00'),
(2,4,6,'2025-12-03 15:30:00'),
(3,5,8,'2025-12-05 09:15:00'),
(4,6,11,'2025-12-06 12:45:00');
/*!40000 ALTER TABLE `UserFavorites` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Notifications`
--

DROP TABLE IF EXISTS `Notifications`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Notifications` (
  `id` int NOT NULL AUTO_INCREMENT,
  `userId` int NOT NULL,
  `title` varchar(120) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `message` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `linkUrl` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `isRead` tinyint(1) DEFAULT '0',
  `createdAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_notifications_user_time` (`userId`,`createdAt`),
  KEY `idx_notifications_user_unread` (`userId`,`isRead`,`createdAt`),
  CONSTRAINT `Notifications_fk_user` FOREIGN KEY (`userId`) REFERENCES `Users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Notifications`
--

LOCK TABLES `Notifications` WRITE;
/*!40000 ALTER TABLE `Notifications` DISABLE KEYS */;
INSERT INTO `Notifications` (`id`,`userId`,`title`,`message`,`isRead`,`createdAt`) VALUES
(1,4,'Новый статус заявки','По объекту #1 назначена встреча с риелтором.',0,'2025-12-05 11:45:00'),
(2,5,'Показ подтвержден','Показ по объекту #8 подтвержден на субботу, 14:00.',1,'2025-12-06 10:10:00');
/*!40000 ALTER TABLE `Notifications` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `AuditLogs`
--

DROP TABLE IF EXISTS `AuditLogs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `AuditLogs` (
  `id` int NOT NULL AUTO_INCREMENT,
  `actorUserId` int DEFAULT NULL,
  `entityType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `entityId` int DEFAULT NULL,
  `action` varchar(80) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `oldValue` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `newValue` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `createdAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_audit_entity_time` (`entityType`,`entityId`,`createdAt`),
  KEY `idx_audit_actor_time` (`actorUserId`,`createdAt`),
  CONSTRAINT `AuditLogs_fk_actor` FOREIGN KEY (`actorUserId`) REFERENCES `Users` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

LOCK TABLES `AuditLogs` WRITE;
/*!40000 ALTER TABLE `AuditLogs` DISABLE KEYS */;
INSERT INTO `AuditLogs` VALUES
(1,3,'Interaction',5,'status-change','2','5','2025-11-18 16:30:00'),
(2,3,'RealEstate',8,'status-change','Забронирован','Продан','2025-11-18 16:30:00'),
(3,2,'RealEstate',3,'status-change','Активен','Черновик','2025-11-20 09:00:00'),
(4,3,'RealEstate',14,'status-change','Активен','Архив','2025-11-21 12:00:00');
/*!40000 ALTER TABLE `AuditLogs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ViewingSlots`
--

DROP TABLE IF EXISTS `ViewingSlots`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ViewingSlots` (
  `id` int NOT NULL AUTO_INCREMENT,
  `realEstateId` int NOT NULL,
  `agentId` int NOT NULL,
  `clientId` int DEFAULT NULL,
  `startsAt` datetime NOT NULL,
  `endsAt` datetime NOT NULL,
  `status` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'available',
  `notes` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`id`),
  KEY `idx_slots_real_estate` (`realEstateId`),
  KEY `idx_slots_agent_time` (`agentId`,`startsAt`),
  KEY `idx_slots_real_estate_time` (`realEstateId`,`startsAt`),
  KEY `idx_slots_client` (`clientId`),
  CONSTRAINT `ViewingSlots_fk_real_estate` FOREIGN KEY (`realEstateId`) REFERENCES `RealEstate` (`id`) ON DELETE CASCADE,
  CONSTRAINT `ViewingSlots_fk_agent` FOREIGN KEY (`agentId`) REFERENCES `Users` (`id`) ON DELETE CASCADE,
  CONSTRAINT `ViewingSlots_fk_client` FOREIGN KEY (`clientId`) REFERENCES `Users` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ViewingSlots`
--

LOCK TABLES `ViewingSlots` WRITE;
/*!40000 ALTER TABLE `ViewingSlots` DISABLE KEYS */;
INSERT INTO `ViewingSlots` VALUES
(1,1,2,4,'2025-12-14 14:00:00','2025-12-14 14:30:00','booked','Клиент подтвердил присутствие.'),
(2,6,2,NULL,'2025-12-15 18:00:00','2025-12-15 18:30:00','available','Свободный вечерний слот.'),
(3,8,3,5,'2025-12-16 13:00:00','2025-12-16 13:45:00','booked','Показ с обсуждением ипотеки.');
/*!40000 ALTER TABLE `ViewingSlots` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ChatMessages`
--

DROP TABLE IF EXISTS `ChatMessages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ChatMessages` (
  `id` int NOT NULL AUTO_INCREMENT,
  `realEstateId` int NOT NULL,
  `senderId` int NOT NULL,
  `recipientId` int NOT NULL,
  `message` varchar(4000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `sentAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `readAt` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_chat_real_estate` (`realEstateId`),
  KEY `idx_chat_sender` (`senderId`),
  KEY `idx_chat_recipient` (`recipientId`),
  KEY `idx_chat_dialog_time` (`senderId`,`recipientId`,`sentAt`),
  KEY `idx_chat_time` (`sentAt`),
  CONSTRAINT `ChatMessages_fk_real_estate` FOREIGN KEY (`realEstateId`) REFERENCES `RealEstate` (`id`) ON DELETE CASCADE,
  CONSTRAINT `ChatMessages_fk_sender` FOREIGN KEY (`senderId`) REFERENCES `Users` (`id`) ON DELETE CASCADE,
  CONSTRAINT `ChatMessages_fk_recipient` FOREIGN KEY (`recipientId`) REFERENCES `Users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ChatMessages`
--

LOCK TABLES `ChatMessages` WRITE;
/*!40000 ALTER TABLE `ChatMessages` DISABLE KEYS */;
INSERT INTO `ChatMessages` VALUES
(1,1,4,2,'Здравствуйте! Удобно посмотреть квартиру в субботу?', '2025-12-05 11:40:00', '2025-12-05 11:42:00'),
(2,1,2,4,'Да, могу в 14:00. Подтверждаю показ.', '2025-12-05 11:44:00', NULL),
(3,8,5,3,'Добрый день, пришлите пожалуйста планировку.', '2025-12-06 09:10:00', NULL);
/*!40000 ALTER TABLE `ChatMessages` ENABLE KEYS */;
UNLOCK TABLES;

/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;
/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-12-08
