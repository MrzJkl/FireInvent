import 'dart:convert';
import 'package:flameguardlaundry/main.dart';
import 'package:http/http.dart' as http;
import 'package:web/web.dart' as web;
import 'package:flameguardlaundry/constants.dart';
import 'package:flutter/material.dart';
import 'package:flutter_login/flutter_login.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  Future<String?> _authUser(LoginData data) async {
    final response = await http.post(
      Uri.parse("${Constants.baseUrl}/auth/login"),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'email': data.name, 'password': data.password}),
    );

    if (response.statusCode == 200) {
      final token = jsonDecode(response.body)['token'];
      web.window.localStorage.setItem('jwt', token);
      return null;
    } else {
      return 'Invalid credentials';
    }
  }

  @override
  Widget build(BuildContext context) {
    return FlutterLogin(
      title: Constants.appName,
      disableCustomPageTransformer: true,
      userType: LoginUserType.email,
      onLogin: _authUser,
      onSubmitAnimationCompleted: () {
        Navigator.of(
          context,
        ).pushReplacement(MaterialPageRoute(builder: (_) => const RootPage()));
      },
      onRecoverPassword: (_) => null,
      hideForgotPasswordButton: true,
    );
  }
}
