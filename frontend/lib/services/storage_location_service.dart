import 'dart:convert';
import 'package:fireinvent/models/create_models/create_storage_location_model.dart';
import 'package:get_it/get_it.dart';

import '../models/storage_location_model.dart';
import '../services/http_service.dart';

class StorageLocationService {
  final HttpService http = GetIt.I<HttpService>();

  Future<List<StorageLocationModel>> getAll() async {
    final response = await http.get('/storageLocations');
    final List<dynamic> json = jsonDecode(response.body);
    return json.map((e) => StorageLocationModel.fromJson(e)).toList();
  }

  Future<StorageLocationModel> getById(String id) async {
    final response = await http.get('/storageLocations/$id');
    return StorageLocationModel.fromJson(jsonDecode(response.body));
  }

  Future<StorageLocationModel> create(CreateStorageLocationModel model) async {
    final response = await http.post('/storageLocations', model.toJson());
    return StorageLocationModel.fromJson(jsonDecode(response.body));
  }

  Future<void> update(String id, StorageLocationModel model) async {
    await http.put('/storageLocations/$id', model.toJson());
  }

  Future<void> delete(String id) async {
    await http.delete('/storageLocations/$id');
  }
}
