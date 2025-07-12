import 'dart:convert';
import 'package:fireinvent/models/create_models/create_maintenance_model.dart';
import 'package:get_it/get_it.dart';

import '../models/maintenance_model.dart';
import '../services/http_service.dart';

class MaintenanceService {
  final HttpService http = GetIt.I<HttpService>();

  Future<List<MaintenanceModel>> getAll() async {
    final response = await http.get('/maintenances');
    final List<dynamic> json = jsonDecode(response.body);
    return json.map((e) => MaintenanceModel.fromJson(e)).toList();
  }

  Future<MaintenanceModel> getById(String id) async {
    final response = await http.get('/maintenances/$id');
    return MaintenanceModel.fromJson(jsonDecode(response.body));
  }

  Future<MaintenanceModel> create(CreateMaintenanceModel model) async {
    final response = await http.post('/maintenances', model.toJson());
    return MaintenanceModel.fromJson(jsonDecode(response.body));
  }

  Future<void> update(String id, MaintenanceModel model) async {
    await http.put('/maintenances/$id', model.toJson());
  }

  Future<void> delete(String id) async {
    await http.delete('/maintenances/$id');
  }
}
