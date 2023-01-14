
# Aventude Assignment Backend (Ecommerce API)


## Execution Plan
* Here is my plan to create the API.

![preview](https://i.imgur.com/oQ8iGHv.png)


## Preprerequisites:
1. MicroSoft SQL Server.
2. Visual Studio 2022.


## Setup Steps
1. Open your MicroSoft SQL Server Management Studio and copy servername. Now go to appsettings.json and paste the servername in the `EcommerceApiConnectionString` connection string under `Server` parameter.

Ex: `"EcommerceApiConnectionString": "Server=[PASTE-SERVERNAME-HERE];Database=EcommerceDb;Trusted_Connection=true;TrustServerCertificate=True"`

2. Start the API. Visual Studio should now open the Swagger documentation. `https://localhost:<port>/swagger/index.html`

3. Try out endpoint `GET /api/admin/create` this will create the admin user with default credentials
	* Email: admin@aventude.com
	* Password: Admin321


## API navigation information
* `GET`: Get a resource information.
* `POST`: Create a resource.
* `PUT`: Update a resource.
* `DELETE`: Delete a resource.


## API Functionalities
1. Customer Registration

2. Admin & Customer Login

3. Product
	* Create product
	* Get all products in store
	* Get single product details
	* Update product details
	* Remove a product from store

4. Category
	* Add Category
	* Get all categories
	* Get single category details
	* Delete category
	* Add product to category
	* Remove a product from category

5. Search: Customers can search for products by product name or category.

6. Order
	* Customer can place order
	* Customer can get order details
	* Admin can edit the status of any customer order

## Preview
* Here is a preview of all endpoints available in the swagger documentation of the API.

![preview](https://i.imgur.com/RMFbKMU.png)

## Support
* If any support or assistance needed please contact Shalinda Fernando.