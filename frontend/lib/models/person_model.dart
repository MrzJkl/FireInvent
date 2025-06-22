import 'package:json_annotation/json_annotation.dart';

part 'person_model.g.dart';

@JsonSerializable()
class PersonModel {
  final String? id;
  final String firstName;
  final String lastName;
  final String? remarks;
  final String? contactInfo;
  final String? externalId;

  PersonModel({
    this.id,
    required this.firstName,
    required this.lastName,
    this.remarks,
    this.contactInfo,
    this.externalId,
  });

  factory PersonModel.fromJson(Map<String, dynamic> json) => _$PersonModelFromJson(json);
  Map<String, dynamic> toJson() => _$PersonModelToJson(this);
}
