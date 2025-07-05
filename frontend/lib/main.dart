import 'package:flameguardlaundry/constants.dart';
import 'package:flameguardlaundry/screens/login_screen.dart';
import 'package:web/web.dart' as web;

import 'package:flameguardlaundry/main_drawer.dart';
import 'package:flameguardlaundry/services/clothing_assigment_service.dart';
import 'package:flameguardlaundry/services/clothing_item_service.dart';
import 'package:flameguardlaundry/services/clothing_product_service.dart';
import 'package:flameguardlaundry/services/clothing_variant_service.dart';
import 'package:flameguardlaundry/services/department_service.dart';
import 'package:flameguardlaundry/services/http_service.dart';
import 'package:flameguardlaundry/services/maintenance_service.dart';
import 'package:flameguardlaundry/services/person_service.dart';
import 'package:flameguardlaundry/services/storage_location_service.dart';
import 'package:flameguardlaundry/services/user_service.dart';
import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';

void main() {
  GetIt.I.registerSingleton<HttpService>(HttpService());
  GetIt.I.registerLazySingleton<ClothingAssignmentService>(
    () => ClothingAssignmentService(),
  );
  GetIt.I.registerLazySingleton<ClothingItemService>(
    () => ClothingItemService(),
  );
  GetIt.I.registerLazySingleton<ClothingProductService>(
    () => ClothingProductService(),
  );
  GetIt.I.registerLazySingleton<ClothingVariantService>(
    () => ClothingVariantService(),
  );
  GetIt.I.registerLazySingleton<DepartmentService>(() => DepartmentService());
  GetIt.I.registerLazySingleton<MaintenanceService>(() => MaintenanceService());
  GetIt.I.registerLazySingleton<PersonService>(() => PersonService());
  GetIt.I.registerLazySingleton<StorageLocationService>(
    () => StorageLocationService(),
  );
  GetIt.I.registerLazySingleton<UserService>(() => UserService());

  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  bool _hasToken() {
    final token = web.window.localStorage.getItem('jwt');
    return token != null && token.isNotEmpty;
  }

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'FlameGuard Laundry',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(
          seedColor: Color.fromARGB(255, 33, 150, 243),
        ),
      ),

      home: _hasToken() ? const RootPage() : const LoginScreen(),
    );
  }
}

class RootPage extends StatefulWidget {
  const RootPage({super.key});

  @override
  State<RootPage> createState() => _RootPageState();
}

class _RootPageState extends State<RootPage> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: Constants.buildMainAppBar(context),
      drawer: MainDrawer(),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: <Widget>[
            Text(
              'Welcome to FlameGuard Laundry!',
              style: Theme.of(context).textTheme.headlineLarge,
            ),
          ],
        ),
      ),
    );
  }
}
