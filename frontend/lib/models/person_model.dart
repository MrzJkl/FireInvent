import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';
import 'create_models/create_person_model.dart';

part 'person_model.g.dart';

@JsonSerializable()
@immutable
class PersonModel extends CreatePersonModel {
  final String id;

  const PersonModel({
    required this.id,
    required super.firstName,
    required super.lastName,
    super.remarks,
    super.contactInfo,
    super.externalId,
  });

  factory PersonModel.fromJson(Map<String, dynamic> json) =>
      _$PersonModelFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$PersonModelToJson(this);
}
