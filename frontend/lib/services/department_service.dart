import 'dart:convert';
import 'package:flameguardlaundry/models/create_models/create_department_model.dart';
import 'package:get_it/get_it.dart';

import '../models/department_model.dart';
import '../services/http_service.dart';

class DepartmentService {
  final HttpService http = GetIt.I<HttpService>();

  Future<List<DepartmentModel>> getAll() async {
    final response = await http.get('/departments');
    final List<dynamic> json = jsonDecode(response.body);
    return json.map((e) => DepartmentModel.fromJson(e)).toList();
  }

  Future<DepartmentModel> getById(String id) async {
    final response = await http.get('/departments/$id');
    return DepartmentModel.fromJson(jsonDecode(response.body));
  }

  Future<DepartmentModel> create(CreateDepartmentModel model) async {
    final response = await http.post('/departments', model.toJson());
    return DepartmentModel.fromJson(jsonDecode(response.body));
  }

  Future<void> update(String id, DepartmentModel model) async {
    await http.put('/departments/$id', model.toJson());
  }

  Future<void> delete(String id) async {
    await http.delete('/departments/$id');
  }
}
