import 'package:json_annotation/json_annotation.dart';

part 'storage_location_model.g.dart';

@JsonSerializable()
class StorageLocationModel {
  final String? id;
  final String name;
  final String? remarks;

  StorageLocationModel({this.id, required this.name, this.remarks});

  factory StorageLocationModel.fromJson(Map<String, dynamic> json) => _$StorageLocationModelFromJson(json);
  Map<String, dynamic> toJson() => _$StorageLocationModelToJson(this);
}
