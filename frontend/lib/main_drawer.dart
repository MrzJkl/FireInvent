import 'package:fireinvent/screens/clothing_product_list_screen.dart';
import 'package:fireinvent/screens/person_list_screen.dart';
import 'package:fireinvent/screens/department_list_screen.dart';
import 'package:fireinvent/screens/storage_location_list_screen.dart';
import 'package:flutter/material.dart';

class MainDrawer extends StatefulWidget {
  const MainDrawer({super.key});

  @override
  State<MainDrawer> createState() => _MainDrawerState();
}

class _MainDrawerState extends State<MainDrawer> {
  @override
  Widget build(BuildContext context) {
    return Drawer(
      child: ListView(
        padding: EdgeInsets.zero,
        children: <Widget>[
          DrawerHeader(
            decoration: BoxDecoration(color: Colors.blue),
            child: Text(
              'Menu',
              style: TextStyle(color: Colors.white, fontSize: 24),
            ),
          ),
          ListTile(
            leading: Icon(Icons.shopping_bag),
            title: Text('Products'),
            onTap: () {
              Navigator.of(context).pushReplacement(
                MaterialPageRoute(
                  builder: (_) => const ClothingProductListScreen(),
                ),
              );
            },
          ),
          ListTile(
            leading: Icon(Icons.people),
            title: Text('Persons'),
            onTap: () {
              Navigator.of(context).pushReplacement(
                MaterialPageRoute(builder: (_) => const PersonListScreen()),
              );
            },
          ),
          ListTile(
            leading: Icon(Icons.storage),
            title: Text('Storage Locations'),
            onTap: () {
              Navigator.of(context).pushReplacement(
                MaterialPageRoute(
                  builder: (_) => const StorageLocationListScreen(),
                ),
              );
            },
          ),
          ListTile(
            leading: Icon(Icons.house),
            title: Text('Departments'),
            onTap: () {
              Navigator.of(context).pushReplacement(
                MaterialPageRoute(builder: (_) => const DepartmentListScreen()),
              );
            },
          ),
        ],
      ),
    );
  }
}
