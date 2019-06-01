using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
	public class EmployeesRecord
	{
		public int EmployeeID {get; set;}
		public string LastName {get; set;}
		public string FirstName {get; set;}
		public string Title {get; set;}
		public string TitleOfCourtesy {get; set;}
		public object BirthDate {get; set;}
		public object HireDate {get; set;}
		public string Address {get; set;}
		public string City {get; set;}
		public string Region {get; set;}
		public string PostalCode {get; set;}
		public string Country {get; set;}
		public string HomePhone {get; set;}
		public string Extension {get; set;}
		public object Photo {get; set;}
		public object Notes {get; set;}
		public int ReportsTo {get; set;}
		public string PhotoPath {get; set;}
	}
	public class CategoriesRecord
	{
		public int CategoryID {get; set;}
		public string CategoryName {get; set;}
		public object Description {get; set;}
		public object Picture {get; set;}
	}
	public class ShippersRecord
	{
		public int ShipperID {get; set;}
		public string CompanyName {get; set;}
		public string Phone {get; set;}
	}
	public class SuppliersRecord
	{
		public int SupplierID {get; set;}
		public string CompanyName {get; set;}
		public string ContactName {get; set;}
		public string ContactTitle {get; set;}
		public string Address {get; set;}
		public string City {get; set;}
		public string Region {get; set;}
		public string PostalCode {get; set;}
		public string Country {get; set;}
		public string Phone {get; set;}
		public string Fax {get; set;}
		public object HomePage {get; set;}
	}
	public class OrdersRecord
	{
		public int OrderID {get; set;}
		public object CustomerID {get; set;}
		public int EmployeeID {get; set;}
		public object OrderDate {get; set;}
		public object RequiredDate {get; set;}
		public object ShippedDate {get; set;}
		public int ShipVia {get; set;}
		public object Freight {get; set;}
		public string ShipName {get; set;}
		public string ShipAddress {get; set;}
		public string ShipCity {get; set;}
		public string ShipRegion {get; set;}
		public string ShipPostalCode {get; set;}
		public string ShipCountry {get; set;}
	}
	public class ProductsRecord
	{
		public int ProductID {get; set;}
		public string ProductName {get; set;}
		public int SupplierID {get; set;}
		public int CategoryID {get; set;}
		public string QuantityPerUnit {get; set;}
		public object UnitPrice {get; set;}
		public object UnitsInStock {get; set;}
		public object UnitsOnOrder {get; set;}
		public object ReorderLevel {get; set;}
		public object Discontinued {get; set;}
	}
	public class Order_DetailsRecord
	{
		public int OrderID {get; set;}
		public int ProductID {get; set;}
		public object UnitPrice {get; set;}
		public object Quantity {get; set;}
		public object Discount {get; set;}
	}
	public class CustomerCustomerDemoRecord
	{
		public object CustomerID {get; set;}
		public object CustomerTypeID {get; set;}
	}
	public class CustomerDemographicsRecord
	{
		public object CustomerTypeID {get; set;}
		public object CustomerDesc {get; set;}
	}
	public class RegionRecord
	{
		public int RegionID {get; set;}
		public object RegionDescription {get; set;}
	}
	public class TerritoriesRecord
	{
		public string TerritoryID {get; set;}
		public object TerritoryDescription {get; set;}
		public int RegionID {get; set;}
	}
	public class EmployeeTerritoriesRecord
	{
		public int EmployeeID {get; set;}
		public string TerritoryID {get; set;}
	}
	public class CustomersRecord
	{
		public object CustomerID {get; set;}
		public string CompanyName {get; set;}
		public string ContactName {get; set;}
		public string ContactTitle {get; set;}
		public string Address {get; set;}
		public string City {get; set;}
		public string Region {get; set;}
		public string PostalCode {get; set;}
		public string Country {get; set;}
		public string Phone {get; set;}
		public string Fax {get; set;}
	}
}