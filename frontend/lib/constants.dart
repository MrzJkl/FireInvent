import 'package:flameguardlaundry/env.dart';
import 'package:flameguardlaundry/screens/login_screen.dart';
import 'package:flutter/material.dart';
import 'package:web/web.dart' as web;

class Constants {
  static const String appName = 'FlameGuard Laundry';
  static String baseUrl = apiUrl ?? "https://api.fgl.otc.joekel.dev";
  static PreferredSizeWidget buildMainAppBar(BuildContext context) {
    return AppBar(
      backgroundColor: Theme.of(context).colorScheme.inversePrimary,
      title: const Text('FlameGuard Laundry'),
      actions: [
        IconButton(
          icon: const Icon(Icons.logout),
          tooltip: 'Logout',
          onPressed: () {
            web.window.localStorage.removeItem('jwt');
            Navigator.of(context).pushReplacement(
              MaterialPageRoute(builder: (_) => const LoginScreen()),
            );
          },
        ),
      ],
    );
  }
}
